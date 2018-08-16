import React, { Component } from 'react';
import {GoogleCharts} from 'google-charts';

export class TimeLineChart extends Component {
    displayName = TimeLineChart.name
    
    render() {
        return (
            <div id={this.props.graphName}></div>
        );
    }

    componentDidMount() {
        this.drawChart()
    }

    componentDidUpdate() {
        this.drawChart()
    }

    drawChart() {
        GoogleCharts.load(() => {
            this._drawChart(this.props.graphName, this.props.graphData, this.props.graphColors)
        }, 'timeline');
    }

    _drawChart(_id, _data, _colors) {
        const container = document.getElementById(_id);
        const chart = new GoogleCharts.api.visualization.Timeline(container);
        const data = GoogleCharts.api.visualization.arrayToDataTable(_data);

        var line_ids = new Array(_data.length);
        _data.forEach(function(d) {
            line_ids.push(d[0]);
        });
        var unique_line_ids = line_ids.filter( (value, index, self) => {
            return self.indexOf(value) === index;
        });
        const height = unique_line_ids.length * 41 + 40;
        chart.draw(data, {
            width: 1800, 
            height: height,
            tooltip: {isHtml: true},
            colors: _colors,
        });
    }
}