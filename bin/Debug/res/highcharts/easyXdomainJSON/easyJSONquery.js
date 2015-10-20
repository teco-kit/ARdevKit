function query(dataPath, plugin)
{
	console.log("Try to load data");
	$.getJSON(dataPath, function(data)
	{
		plugin.options.series[0].data = data;
		$('#' + plugin.id).highcharts(plugin.options);
	})
	.fail(function() { console.log("Failed to load data for " + plugin.id)})
	.done(function() { console.log("Loaded data for " + plugin.id + " successfully")});
};