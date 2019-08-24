﻿import React, { Component } from 'react';
import { render } from 'react-dom';
import { Button } from '../Components/Button';
import { FormGroup, Input } from '../Components/FormControl';
import $ from 'jquery';

const baseUri = "/reset-password";

export default class RequestPasswordReset extends Component {
    constructor(props) {
        super(props);

        this.state = {
            requestFailed: false,
            requestPending: false,
            emailSent: false
        }
    }

    attemptRequest(e) {
        e.preventDefault();

        this.setState({
            requestPending: true
        });

        if (this.state.emailSent)
            return;

        $.ajax({
            type: 'post',
            url: `${baseUri}`,
            data: $("form").serialize()
        }).done(() => {
            this.setState({
                requestPending: false,
                emailSent: true,
                requestFailed: false
            });
        }).fail((err) => {
            this.setState({
                requestPending: false,
                requestFailed: err.responseText
            });

            console.error(`[RequestPasswordReset@attemptrequest] Error requesting new password: `, err.responseText);
        });
    }

    render() {
        let requestFailed;
        if (this.state.requestFailed) {
            requestFailed = (
                <FormGroup>
                    <p className="text-danger">Something went wrong. Please try again.</p>
                </FormGroup>
            )
        }

        let emailSent;
        if (this.state.emailSent) {
            emailSent = (
                <FormGroup>
                    <p className="text-success">We've sent you an email with instructions. It may take up to five minutes to appear.</p>
                </FormGroup>
            )
        }

        let btn_submit;
        if (!this.state.emailSent) {
            btn_submit = (
                <FormGroup>
                    <Button btnClass={`btn btn-primary btn-block`} type="submit" pending={this.state.requestPending}> Reset My Password</Button>
                </FormGroup>
            )
        }

        return (
            <div className="login-clean text-center">
                <form onSubmit={this.attemptRequest.bind(this)}>
                    <h1 className="sr-only">Request password reset form</h1>
                    <h1 className="display-4 mb-5">Password Reset</h1>

                    <FormGroup>
                        <Input type="email" name="email" placeHolder="Email" autoComplete="email" autoFocus required />
                    </FormGroup>

                    {requestFailed}
                    {emailSent}
                    {btn_submit}

                    <a className="forgot" href="/login">Know your details? Login Here</a>
                </form>
            </div>
        );
    }
}


if (document.getElementById('react_requestPasswordreset'))
    render(<RequestPasswordReset />, document.getElementById('react_requestPasswordreset'));