import CoreLocation
import CoreMotion
import Foundation

protocol LocationServiceDelegate: AnyObject {
    func authorizationRestricted()
    func authorizationUknown()
    func promptAuthorizationAction()
    func didAuthorize()
}

@objc public class LocationService: NSObject {
    @objc public static let shared = LocationService()

    weak var delegate: LocationServiceDelegate?
    
    private var locationManager: CLLocationManager!

    private let pedometer = CMPedometer()

    private var lat: Double = 0, lon: Double = 0, alt: Double = 0, distance: Double = 0, steps: Int = 0, tSteps: Int = 0
    
    private var directions: Array<String> = Array()
    
    private var prevCoords: CLLocation = CLLocation()
    
    var enabled: Bool {
        return CLLocationManager.locationServicesEnabled()
    }
    
    init(locationManager: CLLocationManager = CLLocationManager()) {
        super.init()
        self.locationManager = locationManager
        self.locationManager.delegate = self
        self.locationManager.pausesLocationUpdatesAutomatically = false
        self.locationManager.allowsBackgroundLocationUpdates = true
        self.locationManager.activityType = .fitness
        self.locationManager.desiredAccuracy = kCLLocationAccuracyBestForNavigation
    }

    private func startCountingSteps() {
        pedometer.startUpdates(from: Date()) {
            [weak self] pedometerData, error in
            guard let pedometerData = pedometerData, error == nil else { return }

            DispatchQueue.main.async {
                self?.steps = Int(truncating: pedometerData.numberOfSteps) + (self?.tSteps ?? 0)
                print(self?.steps ?? 0)
            }
        }
    }
    
    @objc public func hasAuthorization() -> String {
        let locationStatus: CLAuthorizationStatus
        let locationManager = CLLocationManager()
        let pedometerStatus = CMPedometer.authorizationStatus()
        var permissionState: String = ""
        
        if #available(iOS 14, *) {
            locationStatus = locationManager.authorizationStatus
        } else {
            locationStatus = CLLocationManager.authorizationStatus()
        }
        
        print(locationStatus)
        
        if(locationStatus == .authorizedAlways && pedometerStatus == .authorized){
            permissionState = "granted"
        } else if(locationStatus == .notDetermined && pedometerStatus == .notDetermined){
            permissionState = "neverAsked"
        } else if(locationStatus == .denied && pedometerStatus == .denied){
            permissionState = "denied"
        } else if(pedometerStatus == .denied){
            permissionState = "steps"
        } else if(locationStatus == .denied){
            permissionState = "location"
        } else if(locationStatus == .authorizedWhenInUse || locationStatus == .restricted){
            permissionState = "restrictedLocation"
        }
        
        return permissionState;
    }
    
    @objc public func requestAuthorization() {
        locationManager.requestWhenInUseAuthorization()
        startUpdating()
    }
    
    @objc public func start() {
        locationManager.startUpdatingLocation()
        startUpdating()
    }
    
    @objc public func stop() {
        tSteps = steps
        prevCoords = CLLocation()
        lat = 0
        lon = 0
        alt = 0
        locationManager.stopUpdatingLocation()
        pedometer.stopUpdates()
    }

    @objc func getData() -> Array<Double> {
        return [lat, lon, alt, distance, Double(steps)]
    }
    
    @objc func getRouteCoords() -> Array<String> {
            return directions
    }
    
    @objc public func resetData() {
        lat = 0
        lon = 0
        alt = 0
        distance = 0
        directions.removeAll()
        steps = 0
        tSteps = 0
        prevCoords = CLLocation()
    }

    private func startUpdating() {
        if CMPedometer.isStepCountingAvailable() {
            startCountingSteps()
        }
    }
}

extension LocationService: CLLocationManagerDelegate {
    public func locationManagerDidChangeAuthorization(_ manager: CLLocationManager) {
        let authorizationStatus: CLAuthorizationStatus
        
        if #available(iOS 14, *) {
            authorizationStatus = manager.authorizationStatus
        } else {
            authorizationStatus = CLLocationManager.authorizationStatus()
        }
        
        switch authorizationStatus {
        case .denied:
            print("denied")
            //ask user to authorize
            delegate?.promptAuthorizationAction()
        case .notDetermined:
            print("notDetermined")
        case .restricted:
            print("restricted")
            //inform the user
            self.locationManager.requestAlwaysAuthorization()
            delegate?.authorizationRestricted()
        case .authorizedWhenInUse:
            print("authorizedWhenInUseALOPA")
            self.locationManager.requestAlwaysAuthorization()
            //didAuthorized
            delegate?.didAuthorize()
        case .authorizedAlways:
            print("authorizedAlways")
            //didAuthorized
            delegate?.didAuthorize()
        default:
            print("unknown")
            //inform the user
            delegate?.authorizationUknown()
        }
    }
    
    public func locationManager(_ manager: CLLocationManager, didUpdateLocations locations: [CLLocation]) {
        var tempDistance: Double = 0;
        
        if(lat != 0) {
            tempDistance = prevCoords.distance(from: manager.location ?? prevCoords)
        }
        
        if(lat == 0 || tempDistance >= 5) {
            distance += tempDistance
            prevCoords = manager.location ?? prevCoords
            lat = Double(manager.location?.coordinate.latitude ?? 0)
            lon = Double(manager.location?.coordinate.longitude ?? 0)
            alt = Double(manager.location?.altitude ?? 0)
            
            directions.append("\(lat);\(lon);\(alt)")
        }
        
        print("Updating location: ", locations)
    }
}
