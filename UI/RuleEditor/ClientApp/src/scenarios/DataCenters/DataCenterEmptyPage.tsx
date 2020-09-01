import "./DataCenterListPage.scss";

import * as React from "react";
import { Image } from "office-ui-fabric-react/lib-commonjs/Image";
import { DefaultButton } from "office-ui-fabric-react/lib-commonjs/Button";
import { getValue } from "../../config";
import * as EmptyListImage from "./images/CreateProduct.png";

export const DataCenterEmptyPage: React.StatelessComponent = (): JSX.Element => {
    return (
        <main>
            <section className="datacenter-empty-page">
                <Image alt={"dataCenterListEmptyPage.newImageTItle"} src={EmptyListImage.default} />
                <h1>{"dataCenterListEmptyPage.createProductTitle"}</h1>
                <p>{"dataCenterListEmptyPage.createProductDetails"}</p>
                <DefaultButton
                    primary={true}
                    className="add-product-button"
                    text={"dataCenterListEmptyPage.newProductButton"}
                    onClick={() => {
                        window.open(`${getValue("UI:SERVICETREE_URL")}/main.html#/AddNewService`, "_blank");
                    }}
                />
            </section>
        </main>
    );
};