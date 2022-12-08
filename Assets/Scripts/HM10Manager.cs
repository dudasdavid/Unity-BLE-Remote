/* This is an example to show how to connect to 2 HM-10 devices
 * that are connected together via their serial pins and send data
 * back and forth between them.
 */

using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Text;
using UnityStandardAssets.CrossPlatformInput;
using System;

public class HM10Manager : MonoBehaviour
{
	public string DeviceName = "Empty";
	public string ServiceUUID = "FFE0";
	public string Characteristic = "FFE1";

	public Text HM10_Status;
	public Text BluetoothStatus;
	public Text ReceiveStatus;
	public GameObject Telemetry;
	public GameObject ControlPanel;
	public GameObject ConnectPanel;
	public Text TextToSend;
	public InputField DeviceNameInput;
	public float sendTime = 0.1f;

	enum States
	{
		None,
		Scan,
		Connect,
		Subscribe,
		Unsubscribe,
		Disconnect,
		Communication,
	}

	private bool _workingFoundDevice = true;
	private bool _connected = false;
	private float _timeout = 0f;
	private States _state = States.None;
	private bool _foundID = false;
	private bool connected = false;
	private float joy_x = 0;
	private float joy_y = 0;
	private float joy_R = 0;
	private float joy_Phi = 0;
	private float lastSendTime = 0;
	private int aliveCounter = 0;
	private bool sendButtonFlag = false;
	private int buttonData = 0;


	public float batteryVoltage = 0;



	// this is our hm10 device
	private string _hm10;

	public void OnButton(Button button)
	{
		if (button.name.Contains ("Send"))
		{
			if (string.IsNullOrEmpty (TextToSend.text))
			{
				BluetoothStatus.text = "Enter text to send...";
			}
			else
			{
				SendString (TextToSend.text);
			}
		}
		else if (button.name.Contains("Connect"))
        {
			print(DeviceNameInput.text);
			PlayerPrefs.SetString("Name", DeviceNameInput.text);
			StartProcess();
		}
		else if (button.name.Contains("Stop"))
		{
			sendButtonFlag = true;
			buttonData = 4;
		}
		else if (button.name.Contains("Forward"))
		{
			sendButtonFlag = true;
			buttonData = 1;
		}
		else if (button.name.Contains("Backward"))
		{
			sendButtonFlag = true;
			buttonData = 2;
		}
		else if (button.name.Contains("Reconnect"))
		{
			ControlPanel.SetActive(false);
			ConnectPanel.SetActive(true);
			connected = false;
			SetState(States.Unsubscribe, 1.0f);
			Reset();
			BluetoothLEHardwareInterface.DisconnectAll();
		}
		else if (button.name.Contains("Debug"))
		{
			ControlPanel.SetActive(true);
			ConnectPanel.SetActive(false);
			connected = true;

		}
	}

	void Reset ()
	{
		_workingFoundDevice = false;	// used to guard against trying to connect to a second device while still connecting to the first
		_connected = false;
		_timeout = 0f;
		_state = States.None;
		_foundID = false;
		_hm10 = null;
		//PanelMiddle.SetActive (false);
	}

	void SetState (States newState, float timeout)
	{
		_state = newState;
		_timeout = timeout;
	}

	void StartProcess ()
	{
		DeviceName = DeviceNameInput.text;
		BluetoothStatus.text = "Initializing...";

		Reset ();
		BluetoothLEHardwareInterface.Initialize (true, false, () => {
			
			SetState (States.Scan, 0.1f);
			BluetoothStatus.text = "Initialized";

		}, (error) => {
			
			BluetoothLEHardwareInterface.Log ("Error: " + error);
		});
	}

	// Use this for initialization
	void Start ()
	{
		DeviceNameInput.text = PlayerPrefs.GetString("Name", "CC41-A");
		HM10_Status.text = "";
		BluetoothStatus.text = "";
		ControlPanel.SetActive(false);
		ConnectPanel.SetActive(true);

		//StartProcess ();
	}

	void FixedUpdate()
	{
		if (connected && Time.time - lastSendTime > sendTime) {
			joy_x = CrossPlatformInputManager.GetAxis("Horizontal");
			joy_y = CrossPlatformInputManager.GetAxis("Vertical");
			XY2RPhi(joy_x, joy_y, ref joy_R, ref joy_Phi);
			//Debug.Log(joy_R + "; " + joy_Phi);
			aliveCounter++;
			if (sendButtonFlag)
			{
				SendBytes(new byte[] { 0xFF, (byte)((int)joy_R), (byte)((int)joy_Phi), (byte)buttonData, (byte)(aliveCounter % 16) });
				sendButtonFlag = false;
				lastSendTime = Time.time+1.0f;
			}
			else
			{
				SendBytes(new byte[] { 0xFF, (byte)((int)joy_R), (byte)((int)joy_Phi), 0x00, (byte)(aliveCounter % 16) });
				lastSendTime = Time.time;
			}

		}
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (_timeout > 0f)
		{
			_timeout -= Time.deltaTime;
			if (_timeout <= 0f)
			{
				_timeout = 0f;

				switch (_state)
				{
				case States.None:
					break;

				case States.Scan:
					BluetoothStatus.text = "Scanning for HM10 devices...";

					BluetoothLEHardwareInterface.ScanForPeripheralsWithServices (null, (address, name) => {

						// we only want to look at devices that have the name we are looking for
						// this is the best way to filter out devices
						if (name.Contains (DeviceName))
						{
							_workingFoundDevice = true;

							// it is always a good idea to stop scanning while you connect to a device
							// and get things set up
							BluetoothLEHardwareInterface.StopScan ();
							BluetoothStatus.text = "";

							// add it to the list and set to connect to it
							_hm10 = address;

							HM10_Status.text = "Found HM10";

							SetState (States.Connect, 0.5f);

							_workingFoundDevice = false;
						}

					}, null, false, false);
					break;

				case States.Connect:
					// set these flags
					_foundID = false;

					HM10_Status.text = "Connecting to HM10";

					// note that the first parameter is the address, not the name. I have not fixed this because
					// of backwards compatiblity.
					// also note that I am note using the first 2 callbacks. If you are not looking for specific characteristics you can use one of
					// the first 2, but keep in mind that the device will enumerate everything and so you will want to have a timeout
					// large enough that it will be finished enumerating before you try to subscribe or do any other operations.
					BluetoothLEHardwareInterface.ConnectToPeripheral (_hm10, null, null, (address, serviceUUID, characteristicUUID) => {

						if (IsEqual (serviceUUID, ServiceUUID))
						{
							// if we have found the characteristic that we are waiting for
							// set the state. make sure there is enough timeout that if the
							// device is still enumerating other characteristics it finishes
							// before we try to subscribe
							if (IsEqual (characteristicUUID, Characteristic))
							{
								_connected = true;
								SetState (States.Subscribe, 2f);

								HM10_Status.text = "Connected to HM10";
							}
						}
					}, (disconnectedAddress) => {
						BluetoothLEHardwareInterface.Log ("Device disconnected: " + disconnectedAddress);
						HM10_Status.text = "Disconnected";
					});
					break;

				case States.Subscribe:
					ReceiveStatus.text = "Subscribing to HM10";

					BluetoothLEHardwareInterface.SubscribeCharacteristicWithDeviceAddress (_hm10, ServiceUUID, Characteristic, null, (address, characteristicUUID, bytes) => {

						//BLEFeedback = bytes;
						//
						//BitConverter.ToInt16(bytes, 1);
						ReceiveStatus.text = "Rx: " + ByteArrayToString(bytes);
						Telemetry.GetComponent<TelemetryUpdater>().batteryVoltage = bytes[6] * 2 / 10.0f;
						Telemetry.GetComponent<TelemetryUpdater>().adc_val0 = bytes[7] * 16;
						Telemetry.GetComponent<TelemetryUpdater>().adc_val1 = bytes[8] * 16;
						Telemetry.GetComponent<TelemetryUpdater>().adc_val2 = bytes[9] * 16;
						Telemetry.GetComponent<TelemetryUpdater>().adc_val3 = bytes[10] * 16;
						Telemetry.GetComponent<TelemetryUpdater>().adc_val4 = bytes[11] * 16;
						Telemetry.GetComponent<TelemetryUpdater>().adc_val5 = bytes[12] * 16;
						Telemetry.GetComponent<TelemetryUpdater>().adc_val6 = bytes[13] * 16;
						Telemetry.GetComponent<TelemetryUpdater>().adc_val7 = bytes[14] * 16;
						Telemetry.GetComponent<TelemetryUpdater>().adc_val8 = bytes[15] * 16;
						Telemetry.GetComponent<TelemetryUpdater>().adc_val9 = bytes[16] * 16;

						Telemetry.GetComponent<TelemetryUpdater>().triggers1 = bytes[18];
						Telemetry.GetComponent<TelemetryUpdater>().triggers2 = bytes[19];

						Telemetry.GetComponent<TelemetryUpdater>().posLeft = BitConverter.ToInt16(bytes, 2) * 0.01f;
						Telemetry.GetComponent<TelemetryUpdater>().posRight = BitConverter.ToInt16(bytes, 4) * 0.01f;

						//ReceiveStatus.text = "Received Serial: " + Encoding.UTF8.GetString (bytes);
					});

					// set to the none state and the user can start sending and receiving data
					_state = States.None;
					ReceiveStatus.text = "Waiting...";

					//PanelMiddle.SetActive (true);
					ControlPanel.SetActive(true);
					ConnectPanel.SetActive(false);
					connected = true;
					break;

				case States.Unsubscribe:
					BluetoothLEHardwareInterface.UnSubscribeCharacteristic (_hm10, ServiceUUID, Characteristic, null);
					SetState (States.Disconnect, 4f);
					break;

				case States.Disconnect:
					if (_connected)
					{
						BluetoothLEHardwareInterface.DisconnectPeripheral (_hm10, (address) => {
							BluetoothLEHardwareInterface.DeInitialize (() => {
								
								_connected = false;
								_state = States.None;
							});
						});
					}
					else
					{
						BluetoothLEHardwareInterface.DeInitialize (() => {
							
							_state = States.None;
						});
					}
					break;
				}
			}
		}
	}

	string FullUUID (string uuid)
	{
		return "0000" + uuid + "-0000-1000-8000-00805F9B34FB";
	}
	
	bool IsEqual(string uuid1, string uuid2)
	{
		if (uuid1.Length == 4)
			uuid1 = FullUUID (uuid1);
		if (uuid2.Length == 4)
			uuid2 = FullUUID (uuid2);

		return (uuid1.ToUpper().Equals(uuid2.ToUpper()));
	}

	void SendString(string value)
	{
		var data = Encoding.UTF8.GetBytes (value);
		// notice that the 6th parameter is false. this is because the HM10 doesn't support withResponse writing to its characteristic.
		// some devices do support this setting and it is prefered when they do so that you can know for sure the data was received by 
		// the device
		BluetoothLEHardwareInterface.WriteCharacteristic (_hm10, ServiceUUID, Characteristic, data, data.Length, true, (characteristicUUID) => {

			BluetoothLEHardwareInterface.Log ("Write Succeeded");
		});
	}

	void SendByte (byte value)
	{
		byte[] data = new byte[] { value };
		// notice that the 6th parameter is false. this is because the HM10 doesn't support withResponse writing to its characteristic.
		// some devices do support this setting and it is prefered when they do so that you can know for sure the data was received by 
		// the device
		BluetoothLEHardwareInterface.WriteCharacteristic (_hm10, ServiceUUID, Characteristic, data, data.Length, true, (characteristicUUID) => {
			
			BluetoothLEHardwareInterface.Log ("Write Succeeded");
		});
	}

	void SendBytes(byte[] value)
	{
		byte[] data = value;
		// notice that the 6th parameter is false. this is because the HM10 doesn't support withResponse writing to its characteristic.
		// some devices do support this setting and it is prefered when they do so that you can know for sure the data was received by 
		// the device
		BluetoothLEHardwareInterface.WriteCharacteristic(_hm10, ServiceUUID, Characteristic, data, data.Length, true, (characteristicUUID) => {

			BluetoothLEHardwareInterface.Log("Write Succeeded");
		});
	}

	void XY2RPhi(float x, float y, ref float r, ref float phi)
	{

		r = Mathf.Sqrt(x * x + y * y)*70;
		phi = Mathf.Atan2(y, x);
		if (phi < 0)
		{
			phi += 2 * Mathf.PI;

		}

		phi = phi * 180 / 2 / Mathf.PI;

	}
	public static string ByteArrayToString(byte[] ba)
	{
		return BitConverter.ToString(ba).Replace("-", "");
	}

}
