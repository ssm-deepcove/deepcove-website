﻿import React, { Component } from 'react';
import { Button } from '../../Components/Button';

export class NoticeCard extends Component {
    getClassNames(i) {
        return (`notice ${i ? 'important' : ''}`).trim();
    }

    getReadableDate(d) {
        return new Date(d).toDateString();
    }

    render() {
        const { notice, important } = this.props;

        return (
            <div className={this.getClassNames(important)}>
                <h4>{notice.title}</h4>
                <small>{this.getReadableDate(notice.updated_at)}</small>
                <p>{notice.short_desc}</p>
            </div>
        )
    }
}

export class NoticeSummary extends Component {
    getReadableDate(d) {
        return new Date(d).toDateString();
    }

    render() {
        const { id, noticeboard, active, urgent, title, short_desc, updated_at } = this.props.notice;
        
        let icons = [];
        urgent ? icons.push(<i className="fas fa-exclamation-circle fa-2x mr-2 text-danger" key='urgent' />) : null;
        !active ? icons.push(<i className="fas fa-do-not-enter fa-2x mr-2 text-danger" key='inactive' />) : null;
        !noticeboard.includes("app") ? icons.push(<i className="fad fa-browser fa-2x mr-2" key='web' />) : null;
        !noticeboard.includes("web") ? icons.push(<i className="fab fa-android fa-2x mr-2" key='app' />) : null;

        return (
            <div>
                <small className="float-right">{this.getReadableDate(updated_at)}</small>
                <h4>{title}</h4>
                <p>{short_desc}</p>
                <Button className="btn btn-dark" cb={this.props.cb_edit.bind(this, 1, this.props.notice)}>
                    Edit <i className="fa fa-pencil" />
                </Button>
                {icons}
                <hr />
            </div>
        );
    }
}