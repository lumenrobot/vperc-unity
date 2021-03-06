﻿using UnityEngine;
using System.Collections;
using RabbitMQ.Client;
using Newtonsoft.Json;

public class NaoBehavior : MonoBehaviour {

	IModel channel;
	QueueingBasicConsumer consumer;
	float elapsedTime = 0f;
	float timeToTake = 0f;
	MoveTo lastMoveTo;

	// Use this for initialization
	void Start () {
		ConnectionFactory factory = new ConnectionFactory ();
		factory.UserName = "guest";
		factory.Password = "guest";
		factory.HostName = "localhost";
		factory.VirtualHost = "/";
		Debug.LogFormat ("Connecting to RabbitMQ {0}@{1} {2} ...", 
		                 factory.UserName, factory.HostName, factory.VirtualHost);
		IConnection conn = factory.CreateConnection ();
		Debug.LogFormat ("Connected to RabbitMQ {0}@{1} {2}", 
		                 factory.UserName, factory.HostName, factory.VirtualHost);
		channel = conn.CreateModel ();
		//byte[] body = System.Text.Encoding.UTF8.GetBytes ("{\"name\": \"Hendy\"}");
		//IBasicProperties props = channel.CreateBasicProperties ();
		//channel.BasicPublish ("amq.topic", "avatar.NAO.command", props, body);
		//Debug.Log ("Sent");

		string queue = channel.QueueDeclare ("", false, false, false, null);
		string commandKey = "avatar.NAO.command";
		string humanDetectionKey = "lumen.arkan.human.detection";
		Debug.LogFormat ("Bound queue '{0}' to topic '{1}'", queue, commandKey);
		channel.QueueBind (queue, "amq.topic", commandKey);
		channel.QueueBind (queue, "amq.topic", humanDetectionKey);
		consumer = new QueueingBasicConsumer (channel);
		string consumerTag = channel.BasicConsume (queue, false, consumer);
		Debug.LogFormat ("Queue '{0}' subscribed to topic '{1}' and '{2}' using consumerTag '{3}", 
		                 queue, commandKey, humanDetectionKey, consumerTag);
	}
	
	// Update is called once per frame
	void Update () {
		if (timeToTake > 0f) {
			float progress;
			if (elapsedTime + Time.deltaTime >= timeToTake) {
				progress = (timeToTake - elapsedTime) / timeToTake;
				timeToTake = 0f; // animation done
			} else {
				progress = Time.deltaTime / timeToTake;
			}
			elapsedTime += Time.deltaTime;
			transform.Translate(
				lastMoveTo.RightDistance * progress, 0, -lastMoveTo.BackDistance * progress, Space.Self);
			transform.Rotate(0f, -lastMoveTo.TurnCcwDeg * progress, 0f, Space.Self);
		}

		while (true) {
			RabbitMQ.Client.Events.BasicDeliverEventArgs e = 
				(RabbitMQ.Client.Events.BasicDeliverEventArgs)consumer.Queue.DequeueNoWait (null);
			if (e == null)
				break;

			string bodyStr = System.Text.Encoding.UTF8.GetString(e.Body);
			/*
			Debug.LogFormat ("Got message: appId={0} msgId={1} content={2};{3} {4} {5}", 
			                 e.BasicProperties.AppId, e.BasicProperties.MessageId,
			                 e.BasicProperties.ContentType, e.BasicProperties.ContentEncoding,
			                 e.BasicProperties.Headers.Keys, bodyStr);
			                 */
			JsonSerializerSettings jsonSettings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Objects };
			IDictionary dict = JsonConvert.DeserializeObject<IDictionary>(bodyStr, jsonSettings);
			if (dict["@type"] != null) {
				switch (dict["@type"].ToString()) {
				case "MoveTo":
					lastMoveTo = JsonConvert.DeserializeObject<MoveTo>(bodyStr, jsonSettings);
					Debug.LogFormat ("MoveTo backDistance={0} rightDistance={1} turnCcwDeg={2}",
					                 lastMoveTo.BackDistance, lastMoveTo.RightDistance, lastMoveTo.TurnCcwDeg);
					
					elapsedTime = 0f;
					timeToTake = 10f * Mathf.Sqrt(Mathf.Pow(lastMoveTo.BackDistance, 2) + Mathf.Pow (lastMoveTo.RightDistance, 2));
					/*
				Vector3.Lerp
				transform.position.z -= moveToObj.BackDistance;
				transform.position.x += moveToObj.RightDistance;
				*/
					break;
				case "HumanDetected":
					HumanDetected humanDetected = JsonConvert.DeserializeObject<HumanDetected>(bodyStr, jsonSettings);
					//Debug.LogFormat ("HumanDetected {0} {1}", humanDetected.humanId, humanDetected.position);
					if (HumanDetector.INSTANCE != null) {
						HumanDetector.INSTANCE.OnHumanDetected(humanDetected);
					}
					break;
				case "HumanChanges":
					HumanChanges humanChanges = JsonConvert.DeserializeObject<HumanChanges>(bodyStr, jsonSettings);
					//Debug.LogFormat ("Got HumanChanges {0} + {1}", 
					//                 humanChanges.humanDetecteds.Count, humanChanges.humanMovings.Count);					
					foreach (HumanDetected dd in humanChanges.humanDetecteds)
					{
						HumanDetector.INSTANCE.OnHumanDetected(dd);
					}
					foreach (HumanMoving dd in humanChanges.humanMovings)
					{
						HumanDetector.INSTANCE.OnHumanMoving(dd);
					}
					break;
				}
			}
		}
	}
}
