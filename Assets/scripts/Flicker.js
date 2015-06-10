var minFlickerSpeed : float = 0.01;

var maxFlickerSpeed : float = 0.1;

var minLightIntensity : float = 0;

var maxLightIntensity : float = 1;

 

while (true)

{

     GetComponent.<Light>().enabled = true;

     GetComponent.<Light>().intensity = Random.Range(minLightIntensity, maxLightIntensity);

     yield WaitForSeconds (Random.Range(minFlickerSpeed, maxFlickerSpeed ));

     GetComponent.<Light>().enabled = false;

     yield WaitForSeconds (Random.Range(minFlickerSpeed, maxFlickerSpeed ));

}