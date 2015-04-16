using UnityEngine;
using System.Collections;
using RabbitMQ.Client;

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
		byte[] body = System.Text.Encoding.UTF8.GetBytes ("{\"name\": \"Hendy\"}");
		IBasicProperties props = channel.CreateBasicProperties ();
		channel.BasicPublish ("amq.topic", "avatar.NAO.data.image", props, body);
		Debug.Log ("Sent");

		string queue = channel.QueueDeclare ("", false, false, false, null);
		string imageKey = "avatar.NAO.data.image";
		Debug.LogFormat ("Bound queue '{0}' to topic '{1}'", queue, imageKey);
		channel.QueueBind (queue, "amq.topic", imageKey);
		consumer = new QueueingBasicConsumer (channel);
		string consumerTag = channel.BasicConsume (queue, false, consumer);
		Debug.LogFormat ("Queue '{0}' subscribed to topic '{1}' using consumerTag '{2}", 
		                 queue, imageKey, consumerTag);
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
		}
	}
}
