import React, { Component } from 'react';
import { Route } from 'react-router';
import { Layout } from './components/Layout';
import { Home } from './components/Home';
import { Login } from './components/Login';
import { ProjectDashBoard } from './components/ProjectDashBoard';

export default class App extends Component {
  displayName = App.name

  render() {
    return (
      <Layout>
        <Route exact path='/' component={Home} />
        <Route exact path='/login' component={Login} />
        <Route exact path='/projectdashboard/:board/:sprint' component={ProjectDashBoard} />
      </Layout>
    );
  }
}
