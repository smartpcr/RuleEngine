import "./DataCenterListPage.scss";

import * as React from "react";
import { Image } from "office-ui-fabric-react/lib-commonjs/Image";
import * as NoResultsImage from "./images/NoResults.png";

export const DataCenterListNotMatchPage: React.StatelessComponent = (): JSX.Element => {
    return (
        <div className="datacenter-nomatch-page">
            <Image src={NoResultsImage.default} alt={"dataCenterListNotMatchPage.noMatchImage"} />
            <h1>{"dataCenterListNotMatchPage.noMatchTitle"}</h1>
            <p>{"dataCenterListNotMatchPage.noMatchText"}</p>
        </div>
    );
};