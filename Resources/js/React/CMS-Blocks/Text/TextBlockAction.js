﻿import React, { Component, Fragment } from 'react';
import CMSButton from './TextBlockAction/CMSButton';
import CMSLink from './TextBlockAction/CMSLink';
import EditActionModal from './TextBlockAction/EditActionModal';
import { ConfirmButton } from '../../Components/Button';

export default class TextBlockAction extends Component {

    linkExists() {
        return this.props.link && this.props.link.text;
    }

    clearLink() {
        this.props.editVal('link', null);
    }

    render() {

        let editButton;
        if (this.props.showEditButton) {
            editButton = (
                <EditActionModal
                    link={this.props.link}
                    slotNo={this.props.slotNo}
                    btnText={this.linkExists() ? 'Customize Link' : 'Add Link'}
                    settings={this.props.settings}
                    cb={this.props.editVal.bind(this, 'link')}
                />
            )
        }

        let removeButton;
        if (this.props.showEditButton && this.linkExists()) {
            removeButton = (
                <ConfirmButton btnClass="btn btn-info btn-sm" cb={this.clearLink.bind(this)}>
                    Remove &nbsp; <i className="fas fa-minus" />
                </ConfirmButton>
            )
        }

        let actionElement;
        if (this.linkExists() && !this.props.showEditButton) {
            if (this.props.link.isButton) {
                actionElement = <CMSButton link={this.props.link} />
            } else {
                actionElement = <CMSLink link={this.props.link} />
            }
        }

        return (
            <Fragment>
                {actionElement}
                {editButton}
                {removeButton}
            </Fragment>
        )
    }
}