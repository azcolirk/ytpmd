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
      task_status: new Array(),
      loading: true };

    fetch('api/YouTrackTest/Dashboard')
      .then((response) => {
        return response.json();
      })
      .then(data => {
        console.log(data);
        var new_data = ProjectDashBoard.raw_timeline_data;
        var map = new Map();
        var time_map = new Map();
        data.listdata.forEach(function(e) {
          if (map.has(e.id) === false) {
            map.set(e.id, []);
            time_map.set(e.id, {
              status: e.status, 
              start: new Date(parseInt(e.start) * 1000), 
              end: new Date(parseInt(e.end) * 1000)
            });
          }
          if (time_map.get(e.id).start > new Date(parseInt(e.start) * 1000)) {
            time_map.get(e.id).start = new Date(parseInt(e.start) * 1000);
          }
          if (time_map.get(e.id).end < new Date(parseInt(e.end) * 1000)) {
            time_map.get(e.id).status = e.status;
            time_map.get(e.id).end = new Date(parseInt(e.end) * 1000);
          }
          map.get(e.id).push({
            status: e.status, 
            start: new Date(parseInt(e.start) * 1000), 
            end: new Date(parseInt(e.end) * 1000)
          });
        });

        var local_project_tasks = {
          on_sprint_start: 0,
          added_during_sprint: 0,
          removed_during_sprint: 0,
        };

        var local_task_status = new Array(0);
        local_task_status['Аннулирована'] = 0;
        local_task_status['Открыта'] = 0;
        local_task_status['В обработке'] = 0;
        local_task_status['Подлежит проверке'] = 0;
        local_task_status['В тестировании'] = 0;
        local_task_status['Готово к мержу'] = 0;
        local_task_status['Готово'] = 0;



        map.forEach((value, key, map) => {
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
          } else {
            time_map.delete(key);
          }
        });

        time_map.forEach((value, key, map) => {
          var task_start = new Date(value.start);
          task_start.setHours(0, 0, 0, 0);
          var sprint_start = new Date(parseInt(data.sprintstart) * 1000);
          sprint_start.setHours(0, 0, 0, 0);
          if (task_start > sprint_start) {
            local_project_tasks.added_during_sprint++;
          } else {
            local_project_tasks.on_sprint_start++;
          }
          local_task_status[value.status] = local_task_status[value.status] + 1;
        });

        this.setState({ 
          timeline_data: new_data,
          sprint: data.sprint,
          sprintstart: (new Date(parseInt(data.sprintstart) * 1000)).toLocaleDateString("ru-RU"),
          sprintend: (new Date(parseInt(data.sprintend) * 1000)).toLocaleDateString("ru-RU"),
          project: data.project,
          loading: false,
          project_tasks: local_project_tasks,
          task_status: local_task_status,
        });
      });

    // fetch('api/sprint/tasks')
    //   .then((response) => {
    //     console.log(response);
    //     return response.json();
    //   })
    //   .then(data => {
    //     this.setState({ 
    //       project_tasks:{
    //         on_sprint_start: data.onstart,
    //         added_during_sprint: data.add,
    //         removed_during_sprint: data.del,
    //       }
    //     });
    //   });
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

    var task_status_array = [
      {k: 'Аннулирована', v: state.task_status['Аннулирована']},
      {k: 'Открыта', v: state.task_status['Открыта']},
      {k: 'В обработке', v: state.task_status['В обработке']},
      {k: 'Подлежит проверке', v: state.task_status['Подлежит проверке']},
      {k: 'В тестировании', v: state.task_status['В тестировании']},
      {k: 'Готово к мержу', v: state.task_status['Готово к мержу']},
      {k: 'Готово', v: state.task_status['Готово']},
    ];
    const status_list = task_status_array.map(function(value, id) {
      return <div className="uk-clearfix" data-uk-leader="fill: _" key={id}>
              <div className="uk-float-left">
                <div className="uk-panel" >{value.k}</div>
              </div>
              <div className="uk-float-right">
                <div className="uk-panel">{value.v}</div>
              </div>
            </div>;
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
      {status_list}
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
              {/* <h3>Участники спринта</h3> */}
              {/* <div className="uk-section uk-padding-remove-vertical">
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
              </div> */}
            </div>
          </div>
        </div>  
      );
  }
}
