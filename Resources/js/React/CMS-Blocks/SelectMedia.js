﻿import React, { Component } from 'react';
import Modal from '../Components/Modal';
import { Input } from '../Components/FormControl';
import { Button } from '../Components/Button';
import InfiniteScroll from '../Components/InfiniteScroll';
import $ from 'jquery';

export default class SelectMedia extends Component {
    constructor(props) {
        super(props);

        this.state = {
            filter: this.props.type,
            search: null,
            data: [],
            selectedId: null
        }
    }

    componentDidMount() {
        this.getData();
    }

    getData() {
        $.ajax({
            method: 'get',
            url: `/admin/media/data`
        }).done((data) => {
            this.setState({
                data
            });
        }).fail((err) => {
            console.error(`[SelectMedia@getData] Error getting data: `, err.responseText);
        })
    }

    Filter(x) {
        var result = true;

        if (this.state.filter && x.mediaType.category != this.state.filter)
            result = false;

        if (this.state.search && !x.name.toLowerCase().includes(this.state.search.toLowerCase()))
            result = false;

        return result;
    }

    handleFileSelect() {
        if (!this.props.cb) throw new "A callback is required";

        this.props.cb(
            this.state.data.find(x => x.id == this.state.selectedId) || null
        );

        this.setState({ selectedId: null });
    }

    onInfiniteScrollUpdate() {
        let el = $(`.media-select-icon`).first();
        if (el[0]) $(`.media-select-icon img`).css('height', el[0].offsetWidth);
    }

    render() {
        if (!this.props.showModal) return <div />

        let items = this.state.data.map((media, key) => {
            if (this.Filter(media)) {
                return (
                    <div className="col-lg-4 col-6" key={key}>
                        <Item file={media} selected={media.id == this.state.selectedId} cb={(selectedId) => this.setState({ selectedId })} />
                    </div>
                )
            }
        }).filter(function (element) {
            return element !== undefined;
        });


        let footer = (
            <div className="text-right">
                <Button className="btn btn-dark btn-sm" cb={this.handleFileSelect.bind(this)} dismiss="modal">
                    Okay <i className="fas fa-check-circle" />
                </Button> 
            </div>
        )

        return (
            <Modal size="lg" className="media_select"
                title={this.props.title || `Select an ${this.props.type}`}
                footer={footer}
                handleHideModal={this.props.handleHideModal}
            >
                <Input type="text" value={this.state.search} placeHolder="Search by name..." cb={(search) => this.setState({ search })} />

                <InfiniteScroll onUpdate={this.onInfiniteScrollUpdate}>
                    {items}
                </InfiniteScroll>
            </Modal>
        );
    }
}

class Item extends Component {

    constructor(props) {
        super(props);
        this.state = {
            iconHeight: null
        }
    }    

    render() {

        let mediaType = this.props.file.mediaType.category;
        let imgSrc;

        if (mediaType != 'Image') {
            if (mediaType == 'Audio') imgSrc = "/images/audio.png";
            if (mediaType == 'General') imgSrc = "/images/document.png";
        }
        else imgSrc = `/media?filename=${this.props.file.filename}`;

        return (
            <div className={`${this.props.selected ? 'selected' : ''} media-select-icon bground-primary text-white`}>
                <img src={imgSrc}
                    style={{
                        'width': '100%', 'objectFit': 'contain' }}
                    alt={this.props.file.name}
                    onClick={this.props.cb.bind(this, this.props.file.id)}
                />

                <p className="text-center py-2">{this.props.file.name}</p>
            </div>
        )
    }
}