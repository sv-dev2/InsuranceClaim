//var map;
//function initMap() {
//    map = new google.maps.Map(document.getElementById('map'), {
//        center: { lat: -33.890542, lng: 151.274856 },
//        zoom: 6
//    });

//    var marker = [
//     { lat: -33.890542, lng: 151.274856 },
//     { lat: -33.923036, lng: 151.259052 }
//    ];

//    for (var i = 0; i < marker.length; i++) {
//        addMarker(marker[i]);
//    }


//    function addMarker(props) {
//        var marker = new google.maps.Marker({
//            position: props,
//            map: map
//        });
//    }
//}






// The following example creates complex markers to indicate beaches near
// Sydney, NSW, Australia. Note that the anchor is set to (0,32) to correspond
// to the base of the flagpole.

function initMap() {
    var map = new google.maps.Map(document.getElementById('map'), {
        zoom: 10,
        center: { lat: -17.819616, lng: 31.076335 }
    });

    setMarkers(map);
}

// Data for the markers consisting of a name, a LatLng and a zIndex for the
// order in which these markers should display on top of each other.
var beaches = [
  ['Ok Mbare', -17.857988, 31.041304, 4], //s 
  ['julius nyerere', -17.834635, 31.046519, 5], // s
  ['Ok albion mbuya nehanda', -17.834682, 31.046470 , 3],
  ['bon marche eastlea harare', -17.819616, 31.076335, 2],  //s
  ['Ok chitungwiza', -17.819616, 31.076335, 1],
  ['bone Marche belgravia', -17.794462, 31.044077, 6]  // s
];



function setMarkers(map) {
    // Adds markers to the map.

    // Marker sizes are expressed as a Size of X,Y where the origin of the image
    // (0,0) is located in the top left of the image.

    // Origins, anchor positions and coordinates of the marker increase in the X
    // direction to the right and in the Y direction down.
    var image = {
        url: 'https://developers.google.com/maps/documentation/javascript/examples/full/images/beachflag.png',
        // This marker is 20 pixels wide by 32 pixels high.
        size: new google.maps.Size(20, 32),
        // The origin for this image is (0, 0).
        origin: new google.maps.Point(0, 0),
        // The anchor for this image is the base of the flagpole at (0, 32).
        anchor: new google.maps.Point(0, 32)
    };
    // Shapes define the clickable region of the icon. The type defines an HTML
    // <area> element 'poly' which traces out a polygon as a series of X,Y points.
    // The final coordinate closes the poly by connecting to the first coordinate.
    var shape = {
        coords: [1, 1, 1, 20, 18, 20, 18, 1],
        type: 'poly'
    };

   

    for (var i = 0; i < beaches.length; i++) {

        var beach = beaches[i];
        var marker = new google.maps.Marker({
            position: { lat: beach[1], lng: beach[2] },
            map: map,
           // icon: image,
            shape: shape,
            title: beach[0],
            zIndex: beach[3]
        });
    }
}





function ShowGoogleMap() {
    $('#modelGoogleMap').modal('show')
}