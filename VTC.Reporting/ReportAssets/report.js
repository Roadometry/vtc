 $(function() {
        /** This code runs when everything has been loaded on the page */
        /* Inline sparklines take their values from the contents of the tag */
		
        $('.inlinesparkline').sparkline('html', {type: 'bar', barColor: 'red'} ); 
		
		$('.rtsparkline').sparkline('html',  {
    type: 'line',
    lineColor: '#000000',
    fillColor: false,
    spotColor: '#ff0000',
    minSpotColor: false,
    maxSpotColor: false,
    highlightSpotColor: false,
    highlightLineColor: false}); 
		
		var values = [55, 5];

// Draw a sparkline for the #sparkline element
$('#sparkline').sparkline(values, {
    type: "pie",
    // Map the offset in the list of values to a name to use in the tooltip
    tooltipFormat: '{{offset:offset}} ({{percent.1}}%)',
    tooltipValueLookups: {
        'offset': {
            0: 'First',
            1: 'Second',
            2: 'Third'
        }
    }
});

   });