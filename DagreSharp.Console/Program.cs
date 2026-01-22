using DagreSharp;
using DagreSharp.Console;

Console.WriteLine("DagreSharp");

//var dagre = Example1.Create();
var dagre = ExampleFromDagreJsDocumentation.Create();
// Run the layout
Dagre.IsDebug = true;
dagre.Layout();

foreach (var node in dagre.Nodes)
{
	Console.WriteLine($"Node {node.Id}: x = {node.X}, y = {node.Y}");
}

foreach (var edge in dagre.Edges)
{
	var pointsString = "[";
	foreach (var point in edge.Points)
	{
		pointsString += point.ToString();
	}
	pointsString += "]";
	Console.WriteLine($"Edge {edge.From} -> {edge.To}: points = {pointsString}");
}

Console.ReadLine();

