import React, { Component } from 'react';

export class Home extends Component {
  displayName = Home.name

  make_list(objs, obj_type) {
    return objs.map(function(_obj, id) {
      const sprints = _obj.sprint.slice(0, 3).map(function(obj, id) {
        return <div key={id}>
          <h5 className="uk-margin-remove-bottom"><a href={"/projectdashboard?" + obj_type + "=" + _obj.name + "&sprint=" + obj.name} className="uk-button uk-button-link">{obj.name}</a></h5>
          <p className="uk-text-meta uk-margin-remove-top">задач: {obj.task_count}, {obj.start}-{obj.end}</p>
        </div>
      });

      return <div key={id}> 
        <div className="uk-card uk-card-default">
          <div className="uk-card-header uk-padding-small">
            <div className="uk-grid-small uk-flex-middle" data-uk-grid>
              <div className="uk-width-expand">
                <h3 className="uk-card-title uk-margin-remove-bottom">{_obj.name}</h3>
                <p className="uk-text-meta uk-margin-remove-top">открыто: {_obj.opened}, закрыто: {_obj.closed}</p>
              </div>
            </div>
          </div>
          <div className="uk-card-body uk-padding-small">
            {sprints}
          </div>
          {/* <div className="uk-card-footer uk-padding-small">
            <a href="#" className="uk-button uk-button-link">Поробнее</a>
          </div> */}
        </div>
      </div>
    })
  }

  render() {
    const projects = [
      {name: "Проект 1", opened: 1000, closed: 256, sprint: [
        {name: 'Sprint 1', task_count: 33, start: '2018-01-01', end: '2018-01-31'}, 
        {name: 'Sprint 2', task_count: 36, start: '2018-02-01', end: '2018-02-28'}, 
        {name: 'Sprint 3', task_count: 64, start: '2018-03-01', end: '2018-03-31'}, 
        {name: 'Sprint 4', task_count: 52, start: '2018-04-01', end: '2018-04-30'}
      ]},
      {name: "Проект 2", opened: 200, closed: 77, sprint: [
        {name: 'Sprint 1', task_count: 33, start: '2018-01-01', end: '2018-01-31'}, 
        {name: 'Sprint 2', task_count: 36, start: '2018-02-01', end: '2018-02-28'}, 
        {name: 'Sprint 3', task_count: 64, start: '2018-03-01', end: '2018-03-31'}, 
        {name: 'Sprint 4', task_count: 52, start: '2018-04-01', end: '2018-04-30'}
      ]},
      {name: "Проект 3", opened: 394, closed: 123, sprint: [
        {name: 'Sprint 1', task_count: 33, start: '2018-01-01', end: '2018-01-31'}, 
        {name: 'Sprint 2', task_count: 36, start: '2018-02-01', end: '2018-02-28'}, 
        {name: 'Sprint 3', task_count: 64, start: '2018-03-01', end: '2018-03-31'}, 
        {name: 'Sprint 4', task_count: 52, start: '2018-04-01', end: '2018-04-30'}
      ]},
      {name: "Проект 4", opened: 375, closed: 321, sprint: [
        {name: 'Sprint 1', task_count: 33, start: '2018-01-01', end: '2018-01-31'}, 
        {name: 'Sprint 2', task_count: 36, start: '2018-02-01', end: '2018-02-28'}, 
        {name: 'Sprint 3', task_count: 64, start: '2018-03-01', end: '2018-03-31'}, 
        {name: 'Sprint 4', task_count: 52, start: '2018-04-01', end: '2018-04-30'}
      ]},
    ]

    const boards = [
      {name: "Доска 1", opened: 900, closed: 256, sprint: [
        {name: 'Sprint 1', task_count: 33, start: '2018-01-01', end: '2018-01-31'}, 
        {name: 'Sprint 2', task_count: 36, start: '2018-02-01', end: '2018-02-28'}, 
        {name: 'Sprint 3', task_count: 64, start: '2018-03-01', end: '2018-03-31'}, 
        {name: 'Sprint 4', task_count: 52, start: '2018-04-01', end: '2018-04-30'}
      ]},
      {name: "Доска 2", opened: 500, closed: 77, sprint: [
        {name: 'Sprint 1', task_count: 33, start: '2018-01-01', end: '2018-01-31'}, 
        {name: 'Sprint 2', task_count: 36, start: '2018-02-01', end: '2018-02-28'}, 
        {name: 'Sprint 3', task_count: 64, start: '2018-03-01', end: '2018-03-31'}, 
        {name: 'Sprint 4', task_count: 52, start: '2018-04-01', end: '2018-04-30'}
      ]},
      {name: "Доска 3", opened: 394, closed: 123, sprint: [
        {name: 'Sprint 1', task_count: 33, start: '2018-01-01', end: '2018-01-31'}, 
        {name: 'Sprint 2', task_count: 36, start: '2018-02-01', end: '2018-02-28'}, 
        {name: 'Sprint 3', task_count: 64, start: '2018-03-01', end: '2018-03-31'}, 
        {name: 'Sprint 4', task_count: 52, start: '2018-04-01', end: '2018-04-30'}
      ]},
      {name: "Доска 4", opened: 375, closed: 321, sprint: [
        {name: 'Sprint 1', task_count: 33, start: '2018-01-01', end: '2018-01-31'}, 
        {name: 'Sprint 2', task_count: 36, start: '2018-02-01', end: '2018-02-28'}, 
        {name: 'Sprint 3', task_count: 64, start: '2018-03-01', end: '2018-03-31'}, 
        {name: 'Sprint 4', task_count: 52, start: '2018-04-01', end: '2018-04-30'}
      ]},
    ]

    const project_list = this.make_list(projects, "project");
    const board_list = this.make_list(boards, "board");

    return (
      <div className="uk-padding">
        <div>
          <div className="uk-section uk-padding-remove-vertical">
            <h2>Список проектов и досок</h2>
          </div>
        </div>
        <div className="uk-child-width-1-5 uk-grid-small" data-uk-grid>
          {project_list}
        </div>
        <hr className="uk-divider-icon"/>
        <div className="uk-child-width-1-5 uk-grid-small" data-uk-grid>
          {board_list}
        </div>
        <hr className="uk-divider-icon"/>
      </div>
    );
  }
}
