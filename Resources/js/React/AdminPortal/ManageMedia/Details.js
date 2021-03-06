﻿import React, { Component } from 'react';
import { Button } from '../../Components/Button';
import MetaData from './DetailsWidget/Metadata';
import AudioControls from '../../Components/Audio';

export default class Details extends Component {

    onDelete() {
        this.props.setTab(1);
    }

    render() {
        let imgUrl = `/media?filename=${this.props.data.filename}&original=true`;
        if (this.props.data.mediaType.mime.includes("audio/")) {
            imgUrl = "/images/audio.png";
        }
        if (!this.props.data.mediaType.mime.includes('audio/') && !this.props.data.mediaType.mime.includes('image/'))
            imgUrl = `/images/document.png`;       

        return (
            <div className="row pb-5">
                <div className="col-12 py-3">
                    <div className="pb-4">
                        <Button className="btn btn-dark btn-sm float-left" cb={this.props.setTab.bind(this, 1)}>
                            Back to Gallery
                        </Button>
                        <Button className="btn btn-dark btn-sm float-right" cb={this.props.setTab.bind(this, 3)}>
                            Upload File <i className="fas fa-upload" />
                        </Button>
                    </div>

                    <hr />
                </div>

                
                <div className="col-md-6 col-sm-12 details-container">                   
                    <img className="sticky-top" src={imgUrl} alt={this.props.data.alt} />
                    <AudioControls file={this.props.data} />
                </div>

                <div className="col-md-6 col-sm-12">
                    <MetaData file={this.props.data}
                        alert={this.props.alert}
                        deleteCb={this.props.setTab.bind(this, 1)}
                    />
                </div>
            </div>
        )
    }
}