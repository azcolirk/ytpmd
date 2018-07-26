import React, { Component } from 'react';
import { TimeLineChart } from './TimeLineChart';


function time_to_str(_time) {
  var min = _time % 60;
  var _hour = (_time / 60).toFixed();
  var hour = _hour % 8;
  var _day = (_hour / 8).toFixed();
  var day = _day % 5;
  var week = (_day / 5).toFixed();

  return (week > 0 ? week + "н " : "") + 
         (week > 0 || day > 0 ? day + "д " : "") + 
         (week > 0 || day > 0 || hour > 0 ? hour + "ч " : "") + 
         min + "м";
}

export class ProjectDashBoard extends Component {
  displayName = ProjectDashBoard.name

  static raw_timeline_data = [
    [{label: 'Задача', type: 'string'},
     {label: 'Статус', type: 'string'},
     {label: 'Start', type: 'datetime'},
     {label: 'End', type: 'datetime'}],
    //  [ 't-1000', 'open',      new Date(2018, 7, 1, 8, 30, 0), new Date(2018, 7, 1, 16, 30, 0) ],
    //  [ 't-1000', 'in work',   new Date(2018, 7, 1, 16, 30, 0), new Date(2018, 7, 2, 16, 30, 0) ],
    //  [ 't-1000', 'open',   new Date(2018, 7, 2, 16, 30, 0), new Date(2018, 7, 3, 9, 30, 0) ],
    //  [ 't-1000', 'in work',   new Date(2018, 7, 3, 9, 30, 0), new Date(2018, 7, 3, 17, 30, 0) ],
    //  [ 't-1000', 'wait test',   new Date(2018, 7, 3, 17, 30, 0), new Date(2018, 7, 11, 10, 0, 0) ],
    //  [ 't-1000', 'testing',   new Date(2018, 7, 11, 10, 0, 0), new Date(2018, 7, 12, 17, 0, 0) ],
    //  [ 't-1100', 'open',      new Date(2018, 7, 1, 12, 30, 0), new Date(2018, 7, 1, 16, 30, 0) ],
    //  [ 't-1100', 'in work',   new Date(2018, 7, 1, 16, 30, 0), new Date(2018, 7, 2, 17, 30, 0) ],
    //  [ 't-1100', 'wait test',   new Date(2018, 7, 2, 17, 30, 0), new Date(2018, 7, 5, 10, 0, 0) ],
    //  [ 't-1100', 'testing',   new Date(2018, 7, 5, 10, 0, 0), new Date(2018, 7, 6, 17, 0, 0) ],
    //  [ 't-1200', 'open',      new Date(2018, 7, 1, 8, 30, 0), new Date(2018, 7, 2, 16, 30, 0) ],
    //  [ 't-1200', 'in work',   new Date(2018, 7, 2, 16, 30, 0), new Date(2018, 7, 3, 17, 30, 0) ],
    //  [ 't-1200', 'wait test',   new Date(2018, 7, 3, 17, 30, 0), new Date(2018, 7, 12, 17, 0, 0) ],
    //  [ 't-1210', 'open',      new Date(2018, 7, 1, 8, 30, 0), new Date(2018, 7, 2, 16, 30, 0) ],
    //  [ 't-1210', 'in work',   new Date(2018, 7, 2, 16, 30, 0), new Date(2018, 7, 3, 17, 30, 0) ],
    //  [ 't-1210', 'wait test',   new Date(2018, 7, 3, 17, 30, 0), new Date(2018, 7, 12, 17, 0, 0) ],
    //  [ 't-1220', 'open',      new Date(2018, 7, 1, 8, 30, 0), new Date(2018, 7, 2, 16, 30, 0) ],
    //  [ 't-1220', 'in work',   new Date(2018, 7, 2, 16, 30, 0), new Date(2018, 7, 3, 17, 30, 0) ],
    //  [ 't-1220', 'wait test',   new Date(2018, 7, 3, 17, 30, 0), new Date(2018, 7, 12, 17, 0, 0) ],
    //  [ 't-1230', 'open',      new Date(2018, 7, 1, 8, 30, 0), new Date(2018, 7, 2, 16, 30, 0) ],
    //  [ 't-1230', 'in work',   new Date(2018, 7, 2, 16, 30, 0), new Date(2018, 7, 3, 17, 30, 0) ],
    //  [ 't-1230', 'wait test',   new Date(2018, 7, 3, 17, 30, 0), new Date(2018, 7, 12, 17, 0, 0) ],
    //  [ 't-1240', 'open',      new Date(2018, 7, 1, 8, 30, 0), new Date(2018, 7, 2, 16, 30, 0) ],
    //  [ 't-1240', 'in work',   new Date(2018, 7, 2, 16, 30, 0), new Date(2018, 7, 3, 17, 30, 0) ],
    //  [ 't-1240', 'wait test',   new Date(2018, 7, 3, 17, 30, 0), new Date(2018, 7, 12, 17, 0, 0) ],
    //  [ 't-1250', 'open',      new Date(2018, 7, 1, 8, 30, 0), new Date(2018, 7, 2, 16, 30, 0) ],
    //  [ 't-1250', 'in work',   new Date(2018, 7, 2, 16, 30, 0), new Date(2018, 7, 3, 17, 30, 0) ],
    //  [ 't-1250', 'wait test',   new Date(2018, 7, 3, 17, 30, 0), new Date(2018, 7, 12, 17, 0, 0) ],
    //  [ 't-1260', 'open',      new Date(2018, 7, 1, 8, 30, 0), new Date(2018, 7, 2, 16, 30, 0) ],
    //  [ 't-1260', 'in work',   new Date(2018, 7, 2, 16, 30, 0), new Date(2018, 7, 3, 17, 30, 0) ],
    //  [ 't-1260', 'wait test',   new Date(2018, 7, 3, 17, 30, 0), new Date(2018, 7, 12, 17, 0, 0) ],
    //  [ 't-1270', 'open',      new Date(2018, 7, 1, 8, 30, 0), new Date(2018, 7, 2, 16, 30, 0) ],
    //  [ 't-1270', 'in work',   new Date(2018, 7, 2, 16, 30, 0), new Date(2018, 7, 3, 17, 30, 0) ],
    //  [ 't-1270', 'wait test',   new Date(2018, 7, 3, 17, 30, 0), new Date(2018, 7, 12, 17, 0, 0) ],
    //  [ 't-1280', 'open',      new Date(2018, 7, 1, 8, 30, 0), new Date(2018, 7, 2, 16, 30, 0) ],
    //  [ 't-1280', 'in work',   new Date(2018, 7, 2, 16, 30, 0), new Date(2018, 7, 3, 17, 30, 0) ],
    //  [ 't-1280', 'wait test',   new Date(2018, 7, 3, 17, 30, 0), new Date(2018, 7, 12, 17, 0, 0) ],
    //  [ 't-1290', 'open',      new Date(2018, 7, 1, 8, 30, 0), new Date(2018, 7, 2, 16, 30, 0) ],
    //  [ 't-1290', 'in work',   new Date(2018, 7, 2, 16, 30, 0), new Date(2018, 7, 3, 17, 30, 0) ],
    //  [ 't-1290', 'wait test',   new Date(2018, 7, 3, 17, 30, 0), new Date(2018, 7, 12, 17, 0, 0) ],
    //  [ 't-1201', 'open',      new Date(2018, 7, 1, 8, 30, 0), new Date(2018, 7, 2, 16, 30, 0) ],
    //  [ 't-1201', 'in work',   new Date(2018, 7, 2, 16, 30, 0), new Date(2018, 7, 3, 17, 30, 0) ],
    //  [ 't-1201', 'wait test',   new Date(2018, 7, 3, 17, 30, 0), new Date(2018, 7, 12, 17, 0, 0) ],
    //  [ 't-1202', 'open',      new Date(2018, 7, 1, 8, 30, 0), new Date(2018, 7, 2, 16, 30, 0) ],
    //  [ 't-1202', 'in work',   new Date(2018, 7, 2, 16, 30, 0), new Date(2018, 7, 3, 17, 30, 0) ],
    //  [ 't-1202', 'wait test',   new Date(2018, 7, 3, 17, 30, 0), new Date(2018, 7, 12, 17, 0, 0) ],
    //  [ 't-1203', 'open',      new Date(2018, 7, 1, 8, 30, 0), new Date(2018, 7, 2, 16, 30, 0) ],
    //  [ 't-1203', 'in work',   new Date(2018, 7, 2, 16, 30, 0), new Date(2018, 7, 3, 17, 30, 0) ],
    //  [ 't-1203', 'wait test',   new Date(2018, 7, 3, 17, 30, 0), new Date(2018, 7, 12, 17, 0, 0) ],
    //  [ 't-1204', 'open',      new Date(2018, 7, 1, 8, 30, 0), new Date(2018, 7, 2, 16, 30, 0) ],
    //  [ 't-1204', 'in work',   new Date(2018, 7, 2, 16, 30, 0), new Date(2018, 7, 3, 17, 30, 0) ],
    //  [ 't-1204', 'wait test',   new Date(2018, 7, 3, 17, 30, 0), new Date(2018, 7, 12, 17, 0, 0) ],
    //  [ 't-1205', 'open',      new Date(2018, 7, 1, 8, 30, 0), new Date(2018, 7, 2, 16, 30, 0) ],
    //  [ 't-1205', 'in work',   new Date(2018, 7, 2, 16, 30, 0), new Date(2018, 7, 3, 17, 30, 0) ],
    //  [ 't-1205', 'wait test',   new Date(2018, 7, 3, 17, 30, 0), new Date(2018, 7, 12, 17, 0, 0) ],
    //  [ 't-1206', 'open',      new Date(2018, 7, 1, 8, 30, 0), new Date(2018, 7, 2, 16, 30, 0) ],
    //  [ 't-1206', 'in work',   new Date(2018, 7, 2, 16, 30, 0), new Date(2018, 7, 3, 17, 30, 0) ],
    //  [ 't-1206', 'wait test',   new Date(2018, 7, 3, 17, 30, 0), new Date(2018, 7, 12, 17, 0, 0) ],
    //  [ 't-1207', 'open',      new Date(2018, 7, 1, 8, 30, 0), new Date(2018, 7, 2, 16, 30, 0) ],
    //  [ 't-1207', 'in work',   new Date(2018, 7, 2, 16, 30, 0), new Date(2018, 7, 3, 17, 30, 0) ],
    //  [ 't-1207', 'wait test',   new Date(2018, 7, 3, 17, 30, 0), new Date(2018, 7, 12, 17, 0, 0) ],
  ];

  constructor(props) {
    super(props);
    this.state = { timeline_data: [],
      sprint: "Loading...",
      sprintstart: "Loading...",
      sprintend: "Loading...",
      proect: "Loading...",
      loading: true };

    fetch('api/SampleData/WeatherForecasts')
      .then(response => response.json())
      .then(data => {
        var new_data = ProjectDashBoard.raw_timeline_data;
        data.listdata.forEach(function(element) {
          new_data.push([element.id, element.status, new Date(parseInt(element.start) * 1000), new Date(parseInt(element.end) * 1000)]);
        });
        // {id: "ba-295", status: "В обработке", start: "1531094400", end: "1531969869"},…]
        this.setState({ timeline_data: new_data,
          sprint: data.sprint,
          sprintstart: data.sprintstart,
          sprintend: data.sprintend,
          project: data.project,
          loading: false });
      });

    // new Promise(function(resolve, reject) {
    //   setTimeout(function(){
    //     resolve(ProjectDashBoard.raw_timeline_data);
    //   }, 1000);
    // }).then(response => {
    //   this.setState({ timeline_data: response, loading: false });
    // });
  }

  static renderTimeLine(timeline_data) {
    return (
      <TimeLineChart graphName="timeline" graphData={timeline_data}/>
    );
  }
  

  render() {
    const url = window.location.href;
    const project_name = this.state.loading ? 'YTPMD' : this.state.project;
    const sprint_name = this.state.loading ? 'SPRINT' : this.state.sprint;
    const sprint_start_date = this.state.loading ? "Loading..." : this.state.sprintstart;
    const sprint_end_date = this.state.loading ? "Loading..." : this.state.sprintend;
    const title = '"' + project_name + '" - "' + sprint_name + '" (' + sprint_start_date + ' - ' + sprint_end_date + ')';
    const project_employee = {
      'planning' : ['Jake', 'John', 'Ivan'],
      'project' : ['Katrine', 'Vivaldi', 'John'],
      'development' : ['Programmer #1', 'Programmer #2'],
      'testing' : ['Tester #1', 'Tester #2', 'Tester #3', 'Tester #4'],
    };
    const employee_planning = project_employee['planning'].map(function(name, id){
      return <li key={id}>{name}</li>;
    });
    const employee_project = project_employee['project'].map(function(name, id){
      return <li key={id}>{name}</li>;
    });
    const employee_development = project_employee['development'].map(function(name, id){
      return <li key={id}>{name}</li>;
    });
    const employee_testing = project_employee['testing'].map(function(name, id){
      return <li key={id}>{name}</li>;
    });
    const project_tasks = {
      'on_sprint_start': 70,
      'added_during_sprint': 15,
      'removed_during_sprint': 6,
    };
    const project_time = {
      'planning': 7000,
      'project': 1500,
      'development': 60,
      'testing': 6,
    };
    const project_tasks_total =
    <div className="uk-text-meta">
      <div className="uk-clearfix" data-uk-leader="fill: _">
        <div className="uk-float-left">
          <div className="uk-panel" >Задач на начало спринта:</div>
        </div>
        <div className="uk-float-right">
          <div className="uk-panel">{project_tasks['on_sprint_start']} шт.</div>
        </div>
      </div>
      <div className="uk-clearfix" data-uk-leader="fill: _">
        <div className="uk-float-left">
          <div className="uk-panel" >Задач добавлено в спринт:</div>
        </div>
        <div className="uk-float-right">
          <div className="uk-panel">{project_tasks['added_during_sprint']} шт.</div>
        </div>
      </div>
      <div className="uk-clearfix" data-uk-leader="fill: _">
        <div className="uk-float-left">
          <div className="uk-panel" >Задач удалено из спринта:</div>
        </div>
        <div className="uk-float-right">
          <div className="uk-panel">{project_tasks['removed_during_sprint']} шт.</div>
        </div>
      </div>
      <div className="uk-clearfix" data-uk-leader="fill: _">
        <div className="uk-float-left">
          <div className="uk-panel" >Задач на конец спринта:</div>
        </div>
        <div className="uk-float-right">
          <div className="uk-panel">{project_tasks['on_sprint_start'] + project_tasks['added_during_sprint'] - project_tasks['removed_during_sprint']} шт.</div>
        </div>
      </div>
    </div>;

    const project_time_total = 
    <div className="uk-text-meta">
      <div className="uk-clearfix" data-uk-leader="fill: _">
        <div className="uk-float-left">
          <div className="uk-panel" >Время планирования:</div>
        </div>
        <div className="uk-float-right">
          <div className="uk-panel">{time_to_str(project_time['planning'])}</div>
        </div>
      </div>
      <div className="uk-clearfix" data-uk-leader="fill: _">
        <div className="uk-float-left">
          <div className="uk-panel" >Время проектирования:</div>
        </div>
        <div className="uk-float-right">
          <div className="uk-panel">{time_to_str(project_time['project'])}</div>
        </div>
      </div>
      <div className="uk-clearfix" data-uk-leader="fill: _">
        <div className="uk-float-left">
          <div className="uk-panel" >Время разработки:</div>
        </div>
        <div className="uk-float-right">
          <div className="uk-panel">{time_to_str(project_time['development'])}</div>
        </div>
      </div>
      <div className="uk-clearfix" data-uk-leader="fill: _">
        <div className="uk-float-left">
          <div className="uk-panel" >Время тестирования:</div>
        </div>
        <div className="uk-float-right">
          <div className="uk-panel">{time_to_str(project_time['testing'])}</div>
        </div>
      </div>
    </div>;

    let timeline = this.state.loading ? 
      <p className="uk-text-uppercase uk-text-center">Loading...</p>
    : 
      ProjectDashBoard.renderTimeLine(this.state.timeline_data);

    return (
        <div className="uk-padding uk-grid-divider uk-child-width-expand@s" data-uk-grid>
          <div className="uk-width-3-4">
            <div className="uk-section uk-padding-remove-vertical">
              <h2><a className="uk-link-heading" href={ url }>{ title }</a></h2>
              <div className="uk-float-left uk-width-1-5">
                  <div className="uk-panel">{project_tasks_total}</div>
              </div>
              <div className="uk-float-right uk-width-1-5">
                  <div className="uk-panel">{project_time_total}</div>
              </div>
            </div>
            <hr className="uk-divider-icon"/>
            <div className="uk-container">
              {timeline}
            </div>
          </div>
          <div>
            <div className="uk-section uk-padding-remove-vertical">
              <h3>Участники спринта</h3>
              <div className="uk-section uk-padding-remove-vertical">
                <h4>Планирование</h4>
                <div>
                  <ul className="uk-list">
                    {employee_planning}
                  </ul>
                </div>
              </div>
              <div className="uk-section uk-padding-remove-vertical">
                <h4>Проектирование</h4>
                <div>
                  <ul className="uk-list">
                    {employee_project}
                  </ul>
                </div>
              </div>
              <div className="uk-section uk-padding-remove-vertical">
                <h4>Разработка</h4>
                <div>
                  <ul className="uk-list">
                    {employee_development}
                  </ul>
                </div>
              </div>
              <div className="uk-section uk-padding-remove-vertical">
                <h4>Тестирование</h4>
                <div>
                  <ul className="uk-list">
                    {employee_testing}
                  </ul>
                </div>
              </div>
            </div>
          </div>
        </div>  
    );
  }
}
