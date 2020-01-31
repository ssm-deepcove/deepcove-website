﻿import React, { Component, Fragment } from 'react';
import PhonePreview from '../../Components/PhonePreview';
import NoticeboardSection from './NoticeboardSection';
import { Button, ConfirmButton } from '../../Components/Button';
import { FormGroup, Input, TextArea, Checkbox } from '../../Components/FormControl';
import Rselect from 'react-select';

export default class Editor extends Component {
    constructor(props) {
        super(props);

        this.state = this.props.selected || {
            id: 0,
            title: "",
            updated_at: "",
            long_desc: "",
            noticeboard: [],
            urgent: false,
            active: true
        };

        this.options = [{ label: 'Discover Deep Cove', value: 'app' }, { label: 'Website', value: 'web' }];
    }

    componentWillReceiveProps(prevProps) {
        if (prevProps.selected.id != this.state.id) return;
        this.setState(this.props.selected);
    }
   
    getReadableDate(d) {
        return (!!d ? new Date(d) : new Date()).toDateString();
    }

    handleSubmit(e) {
        e.preventDefault();

        let { id, title, updated_at, long_desc, noticeboard, urgent, active } = this.state;

        let notice = {
            id, title, updated_at, long_desc, urgent, active
        };

        // No board selected
        if (noticeboard == null || noticeboard.length <= 0) {
            this.props.alert.error("You must select one or more noticeboards");
            return;
        }

        // Get the enum (string) for noticeboard
        // 'noticeboard' can be either a string (if user hasn't modified)
        // or an array with 1 or 2 elements(if user has modified)
        notice.noticeboard = this.getNoticeboardString(noticeboard);
        
        this.props.cb_submit(id, notice);
    }

    updateVal(key, val) {
        this.setState({
            [key]: val
        });
    }    

    getNoticeboardString(x) {
        if (typeof x == "string") return x;
        if (x.length == 2) return "all";
        return x[0].value;
    }

    getRSelectValue(x) {
        if (typeof x != "string") return x;
        if (x == "all") return this.options;
        return x == "app" ? this.options[0] : this.options[1]
    }

    render() {
        let delete_btn = this.props.selected && this.props.selected.id > 0 ? (
            <ConfirmButton className="btn btn-danger" cb={this.props.cb_delete.bind(this, this.props.selected.id)}>
                Delete <i className="fas fa-trash" />
            </ConfirmButton>
        ) : null;

        return (
            <Fragment>
                <form className="col-md-8 col-sm-12" onSubmit={this.handleSubmit.bind(this)}>
                    <FormGroup label="Title:" htmlFor="notice:title" required>
                        <Input id="notice:title" type="text" value={this.state.title} cb={this.updateVal.bind(this, 'title')} required/>
                    </FormGroup>

                    <FormGroup label="Message:" htmlFor="notice:message" required>
                        <TextArea id="notice:message" value={this.state.long_desc} cb={this.updateVal.bind(this, 'long_desc')} maxLength="1000" rows="7" required/>
                    </FormGroup>

                    <FormGroup label="Noticeboard:" htmlFor="notice:board" required>
                        <Rselect options={this.options} value={this.getRSelectValue(this.state.noticeboard)} onChange={this.updateVal.bind(this, 'noticeboard')} isMulti required />
                    </FormGroup>

                    <FormGroup label="Urgent:" htmlFor="notice:urgent">
                        <Checkbox id="notice:urgent" checked={this.state.urgent == 1} cb={this.updateVal.bind(this, 'urgent')}>
                            <small className="text-danger font-weight-bold">Warning: This option may send an alert to app users phone</small>
                        </Checkbox>
                    </FormGroup>

                    <FormGroup label="Active:" htmlFor="notice:active">
                        <Checkbox id="notice:active" checked={this.state.active == 1} cb={this.updateVal.bind(this, 'active')}>
                            <small className="font-weight-bold">Inactive notices will be hidden from users</small>
                        </Checkbox>
                    </FormGroup>

                    {delete_btn}

                    <div className="float-right">
                        <Button className="btn btn-dark" cb={this.props.cb_edit.bind(this, 0, {})}>
                            Back <i className="fas fa-undo" />
                        </Button>

                        <Button className="btn btn-success border-dark" type="submit">
                            Save <i className="fas fa-check" />
                        </Button>
                    </div>
                </form>

                <div className="col-md-4 col-sm-12">
                    <PhonePreview>
                        <h4 className="text-center">{this.state.title}</h4>
                        <small className="d-block text-center">{this.getReadableDate(this.state.updated_at)}</small>
                        <hr className="indented" />
                        <p className="indented">{this.state.long_desc}</p>
                        <a className="pointer" onClick={this.props.cb_edit.bind(this, 0, {})}>
                            <NoticeboardSection title="Back" />
                        </a>
                    </PhonePreview>
                </div>
            </Fragment>
        )
    }
}