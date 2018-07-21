import React, { Component } from 'react';

export class Login extends Component {
  displayName = Login.name

  constructor(props) {
    super(props);

    this.handleSubmit = this.handleSubmit.bind(this);
  }

  handleSubmit(event) {
    const data = new FormData(event.target);
    alert('A name was submitted: ' + data.get('email'));
    event.preventDefault();
  }

  render() {
    const url = window.location.href

    return (
      <div>
        <div data-uk-grid className="uk-text-center uk-position-center">
            <form onSubmit={this.handleSubmit}>
                <div className="uk-card uk-card-default uk-card-body">
                    <fieldset className="uk-fieldset">
                        <a className="uk-logo" href={ url }>
                            <img data-src="logo.png" className="uk-logo" width="250" height="120" alt="" data-uk-img/>
                        </a>
                        <div className="uk-margin">
                            <div className="uk-inline">
                                <span className="uk-form-icon" data-uk-icon="icon: user"></span>
                                <input required="yes" className="uk-input uk-form-width-large" name="email" type="email" placeholder="Укажите email адрес"/>
                            </div>
                        </div>
                        <div className="uk-margin">
                            <div className="uk-inline">
                                <span className="uk-form-icon" data-uk-icon="icon: lock"></span>
                                <input required="yes" className="uk-input uk-form-width-large" name="password" type="password" placeholder="Пароль"/>
                            </div>
                        </div>
                        <div className="uk-margin">
                            <input className="uk-button uk-form-width-large" type="submit" placeholder="Пароль"/>
                        </div>
                    </fieldset>
                </div>
            </form>
        </div>
      </div>
    );
  }
}
