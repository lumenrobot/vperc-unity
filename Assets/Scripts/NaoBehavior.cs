using UnityEngine;
using System.Collections;
using RabbitMQ.Client;
using Newtonsoft.Json;

public class NaoBehavior : MonoBehaviour {

	IModel channel;
	QueueingBasicConsumer consumer;

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
		Debug.LogFormat ("Bound queue '{0}' to topic '{1}'", queue, commandKey);
		channel.QueueBind (queue, "amq.topic", commandKey);
		consumer = new QueueingBasicConsumer (channel);
		string consumerTag = channel.BasicConsume (queue, false, consumer);
		Debug.LogFormat ("Queue '{0}' subscribed to topic '{1}' using consumerTag '{2}", 
		                 queue, commandKey, consumerTag);
	}
	
	// Update is called once per frame
	void Update () {
		RabbitMQ.Client.Events.BasicDeliverEventArgs e = 
			(RabbitMQ.Client.Events.BasicDeliverEventArgs)consumer.Queue.DequeueNoWait (null);
		if (e != null) {
			string bodyStr = System.Text.Encoding.UTF8.GetString(e.Body);
			Debug.LogFormat ("Got message: appId={0} msgId={1} content={2};{3} {4} {5}", 
			                 e.BasicProperties.AppId, e.BasicProperties.MessageId,
			                 e.BasicProperties.ContentType, e.BasicProperties.ContentEncoding,
			                 e.BasicProperties.Headers.Keys, bodyStr);
			JsonSerializerSettings jsonSettings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Objects };
			MoveTo moveToObj = JsonConvert.DeserializeObject<MoveTo>(bodyStr, jsonSettings);
			Debug.LogFormat ("MoveTo backDistance={0} rightDistance={1} turnCcwDeg={2}",
			                 moveToObj.BackDistance, moveToObj.RightDistance, moveToObj.TurnCcwDeg);
		}
	}
}
