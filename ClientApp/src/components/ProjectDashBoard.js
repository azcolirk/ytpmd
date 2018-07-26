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

function id_to_link(id) {
  return '<a href="http://youtrack.ispsystem.net:8080/issue/' + id + '" target="_blank">' + id + '</a>';
}

export class ProjectDashBoard extends Component {
  displayName = ProjectDashBoard.name

  static raw_timeline_data = [
    [{label: 'Задача', type: 'string'},
     {label: 'Статус', type: 'string'},
     {label: 'Start', type: 'datetime', pattern: 'dd MMM yyyy HH:mm'}, // 17 янв 2018 17:16
     {label: 'End', type: 'datetime', pattern: 'dd MMM yyyy HH:mm'},
    ],
  ];

  constructor(props) {
    super(props);
    this.state = { timeline_data: [],
      sprint: "",
      sprintstart: "",
      sprintend: "",
      proect: "",
      project_tasks: {
        on_sprint_start: 0,
        added_during_sprint: 0,
        removed_during_sprint: 0,
      },
      project_time: {
        planning: 0,
        project: 0,
        development: 0,
        testing: 0,
      },
      loading: true };

    fetch('api/YouTrackTest/Dashboard')
      .then((response) => {
        console.log(response);
        return response.json();
      })
      .then(data => {
        var new_data = ProjectDashBoard.raw_timeline_data;
        var map = new Map();
        data.listdata.forEach(function(e) {
          if (map.has(e.id) === false) {
            console.log("new array for " + e.id);
            map.set(e.id, []);
          }
          map.get(e.id).push({
            status: e.status, 
            start: new Date(parseInt(e.start) * 1000), 
            end: new Date(parseInt(e.end) * 1000)
          });
        });

        console.log(map);



        map.forEach((value, key, map) => {
          console.log(key);
          console.log(value);
          if (value.length > 1 || value[0].status !== "Готово") {
            value.forEach(function(e) {
              new_data.push(
                [
                  key, 
                  e.status, 
                  e.start, 
                  e.end
                ]
              );
            });
          }
        });

        console.log(new_data);

        this.setState({ 
          timeline_data: new_data,
          sprint: data.sprint,
          sprintstart: data.sprintstart,
          sprintend: data.sprintend,
          project: data.project,
          loading: false 
        });
      });
  }

  render() {
    let html = this.state.loading ? 
      <div className="uk-container" >
        <span  data-uk-spinner="ratio: 5" className="uk-position-center uk-text-center"></span>
      </div>
    : 
      ProjectDashBoard.renderAll(this.state);

    return (
      <div>
        {html}
      </div>
    );
  }

  static renderAll(state) {
    const url = window.location.href;
    const project_name = state.project;
    const sprint_name = state.sprint;
    const sprint_start_date = state.sprintstart;
    const sprint_end_date = state.sprintend;
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

    const project_tasks_total =
    <div className="uk-text-meta">
      <div className="uk-clearfix" data-uk-leader="fill: _">
        <div className="uk-float-left">
          <div className="uk-panel" >Задач на начало спринта:</div>
        </div>
        <div className="uk-float-right">
          <div className="uk-panel">{state.project_tasks.on_sprint_start} шт.</div>
        </div>
      </div>
      <div className="uk-clearfix" data-uk-leader="fill: _">
        <div className="uk-float-left">
          <div className="uk-panel" >Задач добавлено в спринт:</div>
        </div>
        <div className="uk-float-right">
          <div className="uk-panel">{state.project_tasks.added_during_sprint} шт.</div>
        </div>
      </div>
      <div className="uk-clearfix" data-uk-leader="fill: _">
        <div className="uk-float-left">
          <div className="uk-panel" >Задач удалено из спринта:</div>
        </div>
        <div className="uk-float-right">
          <div className="uk-panel">{state.project_tasks.removed_during_sprint} шт.</div>
        </div>
      </div>
      <div className="uk-clearfix" data-uk-leader="fill: _">
        <div className="uk-float-left">
          <div className="uk-panel" >Задач на конец спринта:</div>
        </div>
        <div className="uk-float-right">
          <div className="uk-panel">{state.project_tasks.on_sprint_start + state.project_tasks.added_during_sprint - state.project_tasks.removed_during_sprint} шт.</div>
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
          <div className="uk-panel">{time_to_str(state.project_time.planning)}</div>
        </div>
      </div>
      <div className="uk-clearfix" data-uk-leader="fill: _">
        <div className="uk-float-left">
          <div className="uk-panel" >Время проектирования:</div>
        </div>
        <div className="uk-float-right">
          <div className="uk-panel">{time_to_str(state.project_time.project)}</div>
        </div>
      </div>
      <div className="uk-clearfix" data-uk-leader="fill: _">
        <div className="uk-float-left">
          <div className="uk-panel" >Время разработки:</div>
        </div>
        <div className="uk-float-right">
          <div className="uk-panel">{time_to_str(state.project_time.development)}</div>
        </div>
      </div>
      <div className="uk-clearfix" data-uk-leader="fill: _">
        <div className="uk-float-left">
          <div className="uk-panel" >Время тестирования:</div>
        </div>
        <div className="uk-float-right">
          <div className="uk-panel">{time_to_str(state.project_time.testing)}</div>
        </div>
      </div>
    </div>;
    const colors = ['#e30000','#fed74a','#7dbd36', '#ff7bc3', '#92e1d5', '#42a3df', '#246512'];
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
            <div className="uk-container uk-padding-remove uk-margin-remove">
              <TimeLineChart graphName="timeline" graphData={state.timeline_data} graphColors={colors}/>
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
