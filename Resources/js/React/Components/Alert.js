﻿import React, { Component } from 'react';
import { ToastContainer, toast } from 'react-toastify';
//import 'react-toastify/dist/ReactToastify.min.css';
import $ from 'jquery';

const defaultMsg = { ui: null, debug: null };
const colours = ["success", "info", "warning", "danger", "primary", "secondary", "light", "dark"];

export default class Alert extends Component {
    /**
     * Creates the required ref
     * so we can call the alert() 
     * and responseAlert() methods
     */
    componentDidMount() {
        if (this.props.onRef) {
            this.props.onRef(this);
        }
    }

    /**
     * Tides up resources after use
     */
    componentWillUnmount() {
        if (this.props.onRef) {
            this.props.onRef(undefined);
        }
    }

    /**
     * 
     * @param {string} message
     * @param {[object, object]} responseText
     * @param {[function, function]} cb
     */
    info(message, responseText, cb) {
        let msg = !!responseText ? $.parseJSON(responseText) : defaultMsg;

        toast.info(msg.ui || message, {
            _handleOnShow: this._handleOnShow('info', msg.debug, cb)
        });
    }

    /**
     * 
     * @param {string} message
     * @param {[object, object]} responseText
     * @param {[function, function]} cb
     */
    success(message, responseText, cb) {
        let msg = !!responseText ? $.parseJSON(responseText) : defaultMsg;

        toast.success(msg.ui || message, {
            _handleOnShow: this._handleOnShow('success', msg.debug, cb)
        });
    }

    /**
     * 
     * @param {string} message
     * @param {[object, object]} responseText
     * @param {[function, function]} cb
     */
    warning(message, responseText, cb) {
        let msg = !!responseText ? $.parseJSON(responseText) : defaultMsg;

        toast.warning(msg.ui || message, {
            _handleOnShow: this._handleOnShow('warning', msg.debug, cb)
        });
    }

    /**
     * 
     * @param {string} message
     * @param {[object, object]} responseText
     * @param {[function, function]} cb
     */
    error(message, responseText, cb) {
        let msg = !!responseText ? $.parseJSON(responseText) : defaultMsg;

        toast.error(msg.ui || message, {
            _handleOnShow: this._handleOnShow('error', msg.debug, cb)
        });
    }

    /**
     * 
     * @param {string} message
     * @param {[object, object]} responseText
     * @param {[function, function]} cb
     */
    default(message, responseText, cb) {
        let msg = !!responseText ? $.parseJSON(responseText) : defaultMsg;

        toast(msg.ui || message, {
            _handleOnShow: this._handleOnShow('default', msg.debug, cb)
        });
    }

    /**
     * PRIVATE METHOD, logs to console
     * @param {string} level
     * @param {string} debug
     * @param {[function, function]} cb
     */
    _handleOnShow(level, debug, cb) {
        if (!!debug) {
            switch (level) {
                case 'error':
                    console.error(debug);
                    break;

                case 'warn':
                    console.warn(debug);
                    break;

                default:
                    console.log(debug);
            }
        }

        setTimeout(() => {
            if (cb) cb();
        }, 500);
    }

    render() {
        return (
            <div className={this.props.className || null}>
                {this.props.children}

                <ToastContainer
                    position="top-right"
                    autoClose={3 * 1000}
                    hideProgressBar={false}
                    closeOnClick={false}
                    newestOnTop={false}
                    closeButton={false}
                    pauseOnHover
                    rtl={false}
                />
            </div>
        );
    }
}



export class StaticAlert extends Component {
    bgColor() {
        return colours.includes(this.props.type) ? `alert-${this.props.type}` : `alert-primary`;
    }

    className() {
        return this.props.className || `alert ${this.bgColor()}`;
    }

    render() {
        let btn_dismiss = this.props.dismissible || this.props.dismiss ? (
            <button type="button" className="close" data-dismiss="alert" aria-label="Close"><span aria-hidden="true">×</span></button>
        ) : null;

        return (
            <div role="alert" className={this.className()}>
                {btn_dismiss}
                <span>{this.props.children}</span>
            </div>
        );
    }
}