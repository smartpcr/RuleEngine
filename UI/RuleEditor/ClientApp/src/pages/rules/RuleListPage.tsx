import * as React from "react";
import { AllModules } from "@ag-grid-enterprise/all-modules";
import { AgGridReact } from "ag-grid-react";

import "ag-grid-community/dist/styles/ag-grid.css";
import "ag-grid-community/dist/styles/ag-theme-alpine.css";
import { RootState } from "src/store/RootReducer";
import { bindActionCreators, Dispatch } from "redux";
import { connect } from "react-redux";

export interface IRuleListPageProps {

}

class RuleListPage extends React.Component<IRuleListPageProps> {


    public render(): JSX.Element {

        return (
            <div className="rulelist-main">
                <h3>Not implemented</h3>
                <AgGridReact>
                    rowData={rowData}
                    columnDefs={columnDefs}
                    modules={AllModules}
                </AgGridReact>
            </div>
        );
    }
}


function mapStateToProps(state: RootState): Partial<IRuleListPageProps> {
    return {
        currentUser: state.navbar.currentUser,
        isLoading: state.validationRules.isLoading,
        error: state.validationRules.error,
        breadcrumbs: state.validationRules.breadcrumbs,

        ruleSet: state.validationRules.ruleSet,
        items: state.validationRules.rules,
        selectedItem: state.validationRules.currentRule,
        showPanel: state.validationRules.showPanel,
        showDeleteDialog: state.validationRules.showDeleteDialog,
        formMode: state.validationRules.formMode,
        validationPanelViewMode: state.validationRules.rulePanelViewMode,
        showEvalPanel: state.validationRules.showEvalPanel,
        testResult: state.validationRules.testResults,
        filterContext: state.validationRules.filterContext,
        showImportPanel: state.validationRules.showImportPanel,
        isImporting: state.validationRules.isImporting,
        showSecurityPanel: state.validationRules.showSecurityPanel,
        isAdmin: state.validationRules.isAdmin,
        isContributor: state.validationRules.isContributor
    };
}

function mapDispatchToProps(dispatch: Dispatch): Partial<IRuleListPageProps> {
    return {
        actionCreators: bindActionCreators(ActionCreators, dispatch),
        navbarActionCreators: bindActionCreators(NavbarActionCreators, dispatch)
    };
}

export default connect(mapStateToProps, mapDispatchToProps)(RuleListPage);