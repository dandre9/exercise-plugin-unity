//
//  LocationService.swift
//  LocationServicesTutorial
//
//  Created by naz on 2/3/21.
//

import CoreLocation
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

    private var lat: Double = 0, lon: Double = 0
    
    var enabled: Bool {
        return CLLocationManager.locationServicesEnabled()
    }
    
    init(locationManager: CLLocationManager = CLLocationManager()) {
        print("INITIALIZED")
        super.init()
        self.locationManager = locationManager
        self.locationManager.delegate = self
        self.locationManager.pausesLocationUpdatesAutomatically = false
        self.locationManager.allowsBackgroundLocationUpdates = true
        self.locationManager.activityType = .fitness
        self.locationManager.desiredAccuracy = kCLLocationAccuracyBestForNavigation
    }
    
    @objc public func requestAuthorization() {
        locationManager.requestWhenInUseAuthorization()
        locationManager.requestAlwaysAuthorization()
    }
    
    @objc public func start() {
        print("Start Location")
        locationManager.startUpdatingLocation()
    }
    
    @objc public func stop() {
        locationManager.stopUpdatingLocation()
    }

    @objc func getData() -> Array<Double> {
        return [lat, lon, 0, 0, 0]
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
            delegate?.authorizationRestricted()
        case .authorizedWhenInUse:
            print("authorizedWhenInUse")
            DispatchQueue.main.async{
                self.locationManager.requestAlwaysAuthorization()
                self.locationManagerDidChangeAuthorization(manager)
            }
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
        print("Updating location: ", locations)
        lat = Double(manager.location?.coordinate.latitude ?? 0)
        lon = Double(manager.location?.coordinate.longitude ?? 0)
    }
}
