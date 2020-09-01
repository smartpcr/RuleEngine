import "./DataCenterListPage.scss";
import "azure-devops-ui/Core/override.css";
import * as React from "react";
import { bindActionCreators, Dispatch } from "redux";
import { connect } from "react-redux";
import { FilterBar } from "azure-devops-ui/FilterBar";
import { KeywordFilterBarItem } from "azure-devops-ui/TextFilterBarItem";
import { Filter, FILTER_CHANGE_EVENT, IFilterState } from "azure-devops-ui/Utilities/Filter";
import { trackEvent } from "../../metrics";
import { Link } from "../../navigation";
import { DataCenterListGridRow } from "./DataCenterListGridRow";
import { MessageBar, MessageBarType, ShimmeredDetailsList, DetailsListLayoutMode, SelectionMode, IColumn } from "office-ui-fabric-react";
import { getDataCenterListGridRows, sortDataCenterListGridColumn } from "./DataCenterListGridRowTransformer";
import { Icon } from "azure-devops-ui/Icon";
import { DataCenterListNotMatchPage } from "./DataCenterListNotMatchPage";
import { DataCenterEmptyPage } from "./DataCenterEmptyPage";
import { DataCenter } from "src/shared/models/DataCenter";
import * as DataCenterActions from "../../shared/DataCentersStore/DataCentersActions";
import { RootState } from "src/store/RootReducer";
export interface IDataCenterListPageProps {
    error?: Error;
    items: DataCenter[];
    dataCenterActions: typeof DataCenterActions;
    showFilterBar: boolean;
}

export interface IDataCenterListPageState {
    filter: Filter;
    currentList: DataCenterListGridRow[];
    isLoading: boolean;
    showCallout: boolean;
    sortedKey: string;
    isSortedDescending: boolean;
}

export class DataCenterListPage extends React.Component<IDataCenterListPageProps, IDataCenterListPageState> {
    constructor(props: IDataCenterListPageProps) {
        super(props);
        this.state = {
            filter: new Filter({ defaultState: {} }),
            currentList: [],
            isLoading: true,
            showCallout: false,
            sortedKey: "dcName",
            isSortedDescending: false
        };

        this.state.filter.subscribe(this.onFilterChanged, FILTER_CHANGE_EVENT);

        this.onColumnSortClick = this.onColumnSortClick.bind(this);
    }

    public componentWillReceiveProps(nextProps: IDataCenterListPageProps) {
        if (this.props !== nextProps) {
            this.setState({
                currentList: getDataCenterListGridRows(nextProps.items ? nextProps.items : []),
                isLoading: false
            });
        }
    }
    public componentDidMount() {
        this.props.dataCenterActions.loadDataCenters();
    }
    public render(): JSX.Element {
        return (
            <main>
                {
                    (this.props.error) &&
                    <MessageBar messageBarType={MessageBarType.error}>
                        {DataCenterListPage.translateErrorMessage(this.props.error)}
                    </MessageBar>
                }
                <div className="datacenter-list-page">
                    {this.props.showFilterBar && <FilterBar filter={this.state.filter}>
                        <KeywordFilterBarItem filterItemKey="keywords" />
                    </FilterBar>}
                    <div className="datacenter-list">
                        <ShimmeredDetailsList
                            items={this.state.currentList}
                            columns={this.getColumns()}
                            setKey="set"
                            layoutMode={DetailsListLayoutMode.justified}
                            selectionMode={SelectionMode.none}
                            onColumnHeaderClick={this.onColumnSortClick}
                            enableShimmer={this.state.isLoading}
                        />
                        {(!this.state.isLoading && this.state.currentList.length === 0) &&
                            (!!this.state.filter.getFilterItemValue("keywords")
                                ? <DataCenterListNotMatchPage />
                                : <DataCenterEmptyPage />)}
                    </div>
                </div>
            </main>
        );
    }

    private static translateErrorMessage = (error: Error) => {
        if (!error || !error.message) {
            return "";
        }

        if (error.message.startsWith("ErrorCode: 0")) {
            // Connectivity issue
            return "dataCenterListGrid.errors.noConnection";
        } else if (error.message.startsWith("ErrorCode: 4")) {
            // HTTP Response 4xx
            return "dataCenterListGrid.errors.4xx";
        } else if (error.message.startsWith("ErrorCode: 5")) {
            // HTTP Response 5xx
            return "dataCenterListGrid.errors.5xx";
        } else {
            // Other unknown error
            return "dataCenterListGrid.errors.unknown";
        }
    }

    private getColumns(): IColumn[] {
        const columnSortKey = {
            dcName: "dcName",
            dcCode: "dcCode",
            region: "region"
        };

        return [
            {
                key: columnSortKey.dcName,
                name: "dataCenterListGrid.name",
                ariaLabel: "dataCenterListGrid.name",
                fieldName: "name",
                isSorted: this.isSorted(columnSortKey.dcName),
                isSortedDescending: this.isSortedDescending(columnSortKey.dcName),
                isResizable: true,
                minWidth: 100,
                maxWidth: 400,
                headerClassName: "datacenter-list-name-column-header",
                className: "datacenter-list-name-column",
                onRender: (row?: DataCenterListGridRow, index?: number) => {
                    const dcName = row ? row.dcName : "unknown";
                    return (
                        <div data-name="datacenter-list-name" className="datacenter-list-name">
                            <Icon iconName="Product" className="product-icon" />
                            <Link to={`/dc/${dcName}`}>
                                {dcName}
                            </Link>
                        </div>
                    );
                }
            },
            {
                key: columnSortKey.dcCode,
                name: "dataCenterListGrid.code",
                ariaLabel: "dataCenterListGrid.code",
                fieldName: "code",
                isSorted: this.isSorted(columnSortKey.dcCode),
                isSortedDescending: this.isSortedDescending(columnSortKey.dcCode),
                isResizable: true,
                minWidth: 100,
                maxWidth: 200,
                headerClassName: "datacenter-list-code-column-header",
                className: "datacenter-list-code-column",
                onRender: (row?: DataCenterListGridRow, index?: number) => {
                    const colValue: number = row ? row.dcCode : 0;
                    return (<span>{colValue}</span>);
                }
            },
            {
                key: columnSortKey.region,
                name: "dataCenterListGrid.region",
                ariaLabel: "dataCenterListGrid.region",
                fieldName: "region",
                isSorted: this.isSorted(columnSortKey.region),
                isSortedDescending: this.isSortedDescending(columnSortKey.region),
                isResizable: true,
                minWidth: 100,
                maxWidth: 400,
                headerClassName: "datacenter-list-region-column-header",
                className: "datacenter-list-region-column",
                onRender: (row?: DataCenterListGridRow, index?: number) => {
                    const region = row ? row.region : "unknown";
                    return (<span>{region}</span>);
                }
            }
        ];
    }

    private isSorted(columnSortKey: string): boolean {
        return this.state.sortedKey === columnSortKey ? true : false;
    }

    private isSortedDescending(columnSortKey: string): boolean {
        return this.state.sortedKey === columnSortKey ? this.state.isSortedDescending : false;
    }

    private onFilterChanged = (changedState: IFilterState) => {
        const searchKey = changedState.keywords ? changedState.keywords.value : "";
        trackEvent("DataCenterList.filter", { searchKey: searchKey});
        const items = getDataCenterListGridRows(this.props.items || []);
        if (changedState.keywords && changedState.keywords.value) {
            const filteredList = items.filter(item => item.dcName.toLocaleLowerCase().indexOf(searchKey) >= 0);
            this.setState({ currentList: filteredList });
        } else {
            this.setState({ currentList: items });
        }
    }

    private onColumnSortClick = (ev?: React.MouseEvent<HTMLElement>, column?: IColumn) => {
        const columnKey: string = column ? column.key : "name";
        const isSotedDesc: boolean = column ? !column.isSortedDescending : false;
        trackEvent("DataCenterList.sort", { sortColumn: columnKey });
        const rows = sortDataCenterListGridColumn(this.state.currentList, columnKey, isSotedDesc);
        this.setState({
            currentList: rows,
            sortedKey: columnKey,
            isSortedDescending: isSotedDesc
        });
    }
}

function mapStateToProps(state: RootState) {
    return {
        items: state.dataCenters.items,
        error: state.dataCenters.error
    };
}

function mapDispatchToProps(dispatch: Dispatch) {
    return {
        dataCenterActions: bindActionCreators(DataCenterActions, dispatch)
    };
}

export default connect(mapStateToProps, mapDispatchToProps)(DataCenterListPage);