using UnityEngine;
using System.Collections;

public class MoverCocher : MonoBehaviour {
	//coche
	public float MotorForce;           //potencia del motor
	public WheelCollider WheelColFD;   //collider rueda frontal der
	public WheelCollider WheelColFI;   //collider rueda frontal izk
	public WheelCollider WheelColTD;   //collider rueda trasera der
	public WheelCollider WheelColTI;   //collider rueda trasera izk
	public float SteerForce;           //radio de giro
	public float BrakeForce;           //fuerza frenada
	public Rigidbody coche;            //rigidbody coche
	//otros
	public WheelHit hit;               //variable para detectar colisiones
	public float SidewaysSlipTI;       //variable pare recojer valor de derrape
	public float SidewaysSlipTD;       //variable pare recojer valor de derrape
	//antivuelco
	public float AntiRoll = 5000.0f;   //barras antivuelco
	public float travelL=1.0f;
	public float travelR=1.0f;
	public float antiRollForce;
	//marchas
	public float[] GearRatio;
	public int CurrentGear=0;
	public float MaxRPM=4000.0f;
	public float MinRPM=1000.0f;
	public float RPM=0.0f;

	public AudioSource frenomano;
	//public AudioClip frenomano;
	//public AudioSource frenomano;
	// Inicializacion
	void Start () {

		coche.centerOfMass = new Vector3 (0, -0.4f, 0.4f);   //bajamos centro de gravedad
	}
	
	// Cada frame
	void Update () {

		GearSound ();
		detectaderrape ();
		antivuelcolateral();
		conducir ();
		}

	//MARCHAS ---------------------------------------------------------------------------------------------------------------
	void ShiftGears(){

		if ( RPM >= MaxRPM ) {
			if (CurrentGear<4){
				CurrentGear=CurrentGear + 1;
			}
		}	
		if ( RPM <= MinRPM ) {
			if (CurrentGear>0){
				CurrentGear=CurrentGear - 1;
			}
		}
	}

	//CONDUCCION ------------------------------------------------------------------------------------------------------------
	void conducir(){
		//avanzar
		//audio.PlayOneShot(sonido_aceleracion);
		RPM = (WheelColFI.rpm + WheelColFD.rpm)/2 * (1/GearRatio[CurrentGear]);
		ShiftGears (); //calcular marcha
		if (Input.GetAxis ("Vertical") >= 0) { //si voy palante
			WheelColFI.motorTorque = MotorForce / GearRatio [CurrentGear] * Input.GetAxis ("Vertical");
			WheelColFD.motorTorque = MotorForce / GearRatio [CurrentGear] * Input.GetAxis ("Vertical");
		} 
		else { //voy patras
			WheelColFI.motorTorque = MotorForce/4  * Input.GetAxis ("Vertical");
			WheelColFD.motorTorque = MotorForce/4  * Input.GetAxis ("Vertical");
		}
		//girar
		float h = (Input.GetAxis ("Horizontal") * SteerForce) / (rigidbody.velocity.magnitude/2 + 1);	
		WheelColFD.steerAngle = h;
		WheelColFI.steerAngle = h;
		
		//frenar
		if (Input.GetKey (KeyCode.Space)) {
			WheelColFD.brakeTorque = BrakeForce;
			WheelColFI.brakeTorque = BrakeForce;
			WheelColTD.brakeTorque = BrakeForce / 2;
			WheelColTI.brakeTorque = BrakeForce / 2;
		}
		else if (Input.GetKeyUp (KeyCode.Space)) {
			WheelColFD.brakeTorque = 0;
			WheelColFI.brakeTorque = 0;
			WheelColTD.brakeTorque = 0;
			WheelColTI.brakeTorque = 0;
		}

		//freno de mano		
		if (Input.GetKey (KeyCode.A)) {



		
			WheelColFD.brakeTorque = BrakeForce * 2;
			WheelColFI.brakeTorque = BrakeForce * 2;
			WheelColTD.brakeTorque = BrakeForce * 3;
			WheelColTI.brakeTorque = BrakeForce * 3;
		}
		else if (Input.GetKeyUp (KeyCode.A)) {



	
			WheelColFD.brakeTorque = 0;
			WheelColFI.brakeTorque = 0;
			WheelColTD.brakeTorque = 0;
			WheelColTI.brakeTorque = 0;
		}	
	}
	//DETECTAR RUEDA EN PAVIMENTO -------------------------------------------------------------------------------------------
	void detectarposicion(){
		WheelHit hitTI;
		if (WheelColTI.GetGroundHit (out hitTI)) {
			if (hitTI.collider.gameObject.tag == "Finish") {
				Debug.Log ("Coche tocando suelo");
			}
		}
	}
	//DETECTAR DERRAPE RUEDA -----------------------------------------------------------------------------------------------
	void detectaderrape(){
		WheelHit hitTI;
		WheelHit hitTD;
		if (WheelColTI.GetGroundHit (out hitTI)) {
			SidewaysSlipTI = hitTI.sidewaysSlip;
			if(System.Math.Abs(SidewaysSlipTI)<3){
				frenomano.Play ();
			}
		}
		if (WheelColTD.GetGroundHit (out hitTD)) {
			SidewaysSlipTD = hitTD.sidewaysSlip;
			if(System.Math.Abs(SidewaysSlipTD)<3){
				frenomano.Play ();
			}
		}

		}
	//ANTIVUELCO ----------------------------------------------------------------------------------------------------------
	void antivuelcolateral(){
			//frontales
			bool groundedL = WheelColFI.GetGroundHit(out hit);
			
			if (groundedL)
				travelL = (-WheelColFI.transform.InverseTransformPoint(hit.point).y - WheelColFI.radius)/ WheelColFI.suspensionDistance;
			else
				travelL = 1.0f;
			
			bool groundedR = WheelColFD.GetGroundHit(out hit);
			if (groundedR)
				travelR = (-WheelColFD.transform.InverseTransformPoint(hit.point).y - WheelColFD.radius)/ WheelColFD.suspensionDistance;
			else
				travelR = 1.0f;
			
			antiRollForce = (travelL - travelR) * AntiRoll;
			
			if (groundedL)
				rigidbody.AddForceAtPosition(transform.up * -antiRollForce, WheelColFI.transform.position);
			if (groundedR)
				rigidbody.AddForceAtPosition(transform.up * antiRollForce, WheelColFD.transform.position);
			
			//traseras
			bool groundedTL = WheelColTI.GetGroundHit(out hit);
			
			if (groundedTL)
				travelL = (-WheelColTI.transform.InverseTransformPoint(hit.point).y - WheelColTI.radius)/ WheelColTI.suspensionDistance;
			else
				travelL = 1.0f;
			
			bool groundedTR = WheelColTD.GetGroundHit(out hit);
			if (groundedTR)
				travelR = (-WheelColTD.transform.InverseTransformPoint(hit.point).y - WheelColTD.radius)/ WheelColTD.suspensionDistance;
			else
				travelR = 1.0f;
			
			antiRollForce = (travelL - travelR) * AntiRoll;
			
			if (groundedL)
				rigidbody.AddForceAtPosition(transform.up * -antiRollForce, WheelColTI.transform.position);
			if (groundedR)
				rigidbody.AddForceAtPosition(transform.up * antiRollForce, WheelColTD.transform.position);	
		}

	//Sonido
	void GearSound(){
		audio.pitch = System.Math.Abs(RPM / MaxRPM) + 1 ;
		if ( audio.pitch > 2.5f ) {
			audio.pitch = 2.5f;
		}
	}
	//Mostrar info en pantalla
	void OnGUI()
	{
		int rounded = (int)System.Math.Round(rigidbody.velocity.magnitude, 0);
		GUI.Label(new Rect( 450,10, 50,30),""+rounded*3.6f+" km/h");
		int c = CurrentGear + 1;
		GUI.Label(new Rect( 400,10, 50,30),""+c);
	}
}


