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
            this._drawChart(this.props.graphName, this.props.graphData)
        }, 'timeline');
    }

    _drawChart(_id, _data) {
        const container = document.getElementById(_id);
        const chart = new GoogleCharts.api.visualization.Timeline(container);
        const data = GoogleCharts.api.visualization.arrayToDataTable(_data);
        chart.draw(data, {height: 640});
    }
}