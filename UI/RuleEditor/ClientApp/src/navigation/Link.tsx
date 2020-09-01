import * as React from "react";
import * as Router from "react-router";
import { Location, LocationDescriptor } from "history";
import * as PropTypes from "prop-types";
import "./Link.scss";

type ToLocationFunction = (location: Location) => LocationDescriptor;

export class Link extends React.Component<Router.LinkProps> {
    // tslint:disable-next-line:no-any
    public static contextTypes: React.ValidationMap<any> = {
        router: PropTypes.object.isRequired
    };


    constructor(props: Router.LinkProps) {
        super(props);
    }

    public render(): JSX.Element {
        return (
            <Router.Link {...this.props} to={loc => this.resolveRelativeLink(this.props.to, loc)} onClick={e => this.onClick(e)} className={this.props.disabled ? `${this.props.className} disabled-link` : this.props.className} />
        );
    }


    private resolveTo(to: LocationDescriptor | ToLocationFunction, location: Location): LocationDescriptor {
        return typeof to === "function" ? to(location) : to;
    }


    private resolveRelativeLink(to: LocationDescriptor | ToLocationFunction, location: Location): LocationDescriptor {
        let href = this.router.createHref(this.resolveTo(to, location));
        const isAbsolute = /^(?:[a-z]+:)?\/\//i.test(href);
        if (!isAbsolute) {
            href = href;
        }

        return href;
    }


    private get router(): Router.InjectedRouter & { location: Location } {
        return this.context.router;
    }


    private onClick(e: React.MouseEvent) {
        if (this.props.onClick) {
            this.props.onClick(e);
        }
        if (e.defaultPrevented || this.isModifiedEvent(e) || !this.isLeftClickEvent(e)) {
            return;
        }

        this.router.push(this.resolveTo(this.props.to, this.router.location));
        e.preventDefault();
    }


    private isModifiedEvent(e: React.MouseEvent) {
        return !!(e.metaKey || e.altKey || e.ctrlKey || e.shiftKey);
    }


    private isLeftClickEvent(e: React.MouseEvent) {
        return e.button === 0;
    }
}
