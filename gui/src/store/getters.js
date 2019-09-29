import { stat } from "fs";

export default {
    getMetadata: state => state.metadata,
    getAuthenticated: state => state.authenticated,
    getAuthenticationRequired: state => state.authenticationRequired,
    getLastAuthenticateResult: state => state.lastAuthenticateResult,
    getUserToken: state => state.userToken,
    getRequests: state => state.requests,
    getStubs: state => state.stubs,
    getToast: state => state.toast,
    getLastSelectedStub: state => state.lastSelectedStub,
    getTenantNames: state => state.tenantNames,
    getDarkTheme: state => state.settings.darkTheme
};