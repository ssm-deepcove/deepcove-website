﻿import React, { Component } from 'react';
import NavOverview from './Navbar/NavOverview';
import NavSettings from './Navbar/NavSettings';
import $ from 'jquery';

const baseUri = "/admin/settings/navbar";

export default class Navbar extends Component {
    constructor(props) {
        super(props);

        this.state = {
            navbar: {},
            pages: {},
            activeId: 0
        }
    }


    componentDidMount() { this.getData() };

    getData() {
        $.ajax({
            method: 'get',
            url: baseUri
        }).done((navbar) => {
            this.setState({
                navbar
            });
        }).fail((err) => {
            console.err(err);
        })

        $.ajax({
            method: 'get',
            url: `/admin/pages/data?filter=all`
        }).done((pages) => {
            this.setState({
                pages
            });
        })
    }

    setActive(id) {
        this.setState({
            activeId: id
        });
    }

    render() {
        return (
            <div className="row">
                <div className="col-lg-3 col-md-6 col-sm-12">
                    <NavOverview links={this.state.navbar}
                        activeId={this.state.activeId}
                        setActive={this.setActive.bind(this)}
                    />
                    
                </div>

                <div className="col-lg-9 col-md-6 col-sm-12">
                    <NavSettings link={this.state.navbar[this.state.activeId]}
                        pages={this.state.pages}
                    />
                </div>
            </div>
        )
    }
}