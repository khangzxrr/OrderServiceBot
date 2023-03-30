using OrderServiceBot;

var mqListener = new MqLisener();
var mqLisenerThread = new Thread(mqListener.DoWork);
mqLisenerThread.Start();

