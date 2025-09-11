import Vue from 'vue'
import axios from 'axios'
import VueAxios from 'vue-axios'
import router from '../router/index'
import jwtService from './JwtService'

import store from '../store';
import { IS_LOADING } from '../store/modules/base/actions.type'
import { DESTROY_USER } from '../store/modules/auth/actions.type'

const ApiService = {
    init() {
        Vue.use(VueAxios, axios);
        Vue.axios.defaults.baseURL = "http://api.stellacicekcilik.shop/api"
        // Vue.axios.defaults.baseURL = "http://159.253.36.44:8080/api"
        //Vue.axios.defaults.baseURL = "http://localhost:5000/api"
        //Vue.axios.defaults.baseURL = "https://api.evacicekcilik.xyz/api"
        // Vue.axios.defaults.baseURL = "http://localhost:5000/api"
    },

    setHeader() {
        var tokenItem = jwtService.getToken();
        Vue.axios.defaults.headers.common[
            "Authorization"
        ] = `Bearer ${tokenItem.token}`;

        Vue.axios.defaults.headers.common[
            "Accept-Language"
        ] = "tr"
    },

    removeHeader() {
        delete axios.defaults.headers.common["Authorization"];
    },

    post(resource, params) {
        delete Vue.axios.defaults.headers.post["Content-Type"]
        return new Promise((resolve, reject) => {
            store.dispatch(IS_LOADING)

            Vue.axios.post(`${resource}`, params).then((response) => {
                resolve(response.data);
            }).catch((err) => {
                if (!err.response) {
                    const data = {
                        message: ""
                    }
                    reject(data)
                } else {
                    const statusCode = err.response.status;
                    if (statusCode === 401) {
                        store.dispatch(DESTROY_USER)
                        router.push({ path: '/auth/login' })
                        reject(err.response.data)
                    } else if (statusCode === 404) {
                        reject(err.response.data);
                    } else if (statusCode === 500) {
                        err.response.data.message = ''
                        reject(err.response.data);
                    }
                }
            }).finally(() => {
                store.dispatch(IS_LOADING)
            })
        })
    },

    postFile(resource, params) {
        Vue.axios.defaults.headers.post['Content-Type'] = 'application/json';
        return new Promise((resolve, reject) => {
            store.dispatch(IS_LOADING)

            Vue.axios.post(`${resource}`, params).then((response) => {
                resolve(response.data);
            }).catch((err) => {
                if (!err.response) {
                    const data = {
                        message: ""
                    }
                    reject(data)
                } else {
                    const statusCode = err.response.status;
                    if (statusCode === 401) {
                        if (err.response.data.statusCode === undefined) {
                            store.dispatch(DESTROY_USER)
                            router.push({ path: '/auth/login' })
                            err.response.data.message = '';
                        }
                        reject(err.response.data)
                    } else if (statusCode === 404) {
                        reject(err.response.data);
                    } else if (statusCode === 500) {
                        err.response.data.message = ''
                        reject(err.response.data);
                    }
                }
            }).finally(() => {
                store.dispatch(IS_LOADING)
            })
        })
    },

    get(resource, params) {
        delete Vue.axios.defaults.headers.post['Content-Type']
        return new Promise((resolve, reject) => {
            store.dispatch(IS_LOADING);

            Vue.axios.get(`${resource}`, params).then((response) => {
                resolve(response.data);
            }).catch((err) => {
                if (!err.response) {
                    const data = {
                        message: ''
                    };
                    reject(data);
                } else {
                    const statusCode = err.response.status;
                    if (statusCode === 401) {
                        if (err.response.data.statusCode === undefined) {
                            store.dispatch(DESTROY_USER)
                            router.push({ path: '/auth/login' })
                            err.response.data.message = '';
                        }
                        reject(err.response.data)
                    } else if (statusCode === 404) {
                        reject(err.response.data);
                    } else if (statusCode === 500) {
                        err.response.data.message = ''
                        reject(err.response.data);
                    }
                }
            }).finally(() => {
                store.dispatch(IS_LOADING);
            })
        })
    },

    put(resource, params) {
        delete Vue.axios.defaults.headers.post['Content-Type']
        return new Promise((resolve, reject) => {
            store.dispatch(IS_LOADING);

            Vue.axios.put(`${resource}`, params).then((response) => {
                resolve(response.data);
            }).catch((err) => {
                if (!err.response) {
                    const data = {
                        message: ''
                    };
                    reject(data);
                } else {
                    const statusCode = err.response.status;
                    if (statusCode === 401) {
                        if (err.response.data.statusCode === undefined) {
                            store.dispatch(DESTROY_USER)
                            router.push({ path: '/auth/login' })
                            err.response.data.message = '';
                        }
                        reject(err.response.data)
                    } else if (statusCode === 404) {
                        reject(err.response.data);
                    } else if (statusCode === 500) {
                        err.response.data.message = ''
                        reject(err.response.data);
                    }
                }
            }).finally(() => {
                store.dispatch(IS_LOADING);
            })
        })
    },

    delete(resource, params) {
        delete Vue.axios.defaults.headers.post['Content-Type']
        return new Promise((resolve, reject) => {
            store.dispatch(IS_LOADING);

            Vue.axios.delete(`${resource}`, params).then((response) => {
                resolve(response.data);
            }).catch((err) => {
                if (!err.response) {
                    const data = {
                        message: ''
                    };
                    reject(data);
                } else {
                    const statusCode = err.response.status;
                    if (statusCode === 401) {
                        if (err.response.data.statusCode === undefined) {
                            store.dispatch(DESTROY_USER)
                            router.push({ path: '/auth/login' })
                            err.response.data.message = '';
                        }
                        reject(err.response.data)
                    } else if (statusCode === 404) {
                        reject(err.response.data);
                    } else if (statusCode === 500) {
                        err.response.data.message = ''
                        reject(err.response.data);
                    }
                }
            }).finally(() => {
                store.dispatch(IS_LOADING);
            })
        })
    },
}

export default ApiService;