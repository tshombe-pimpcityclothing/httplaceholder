import { createRouter, createWebHashHistory } from "vue-router";
import store from "@/store";

const routes = [
  {
    path: "/",
    name: "Home",
    redirect: "/requests",
  },
  {
    path: "/requests",
    name: "Requests",
    component: () =>
      import(/* webpackChunkName: "requests" */ "../views/Requests.vue"),
  },
  {
    path: "/stubs",
    name: "Stubs",
    component: () =>
      import(/* webpackChunkName: "stubs" */ "../views/Stubs.vue"),
  },
  {
    path: "/stubForm/:stubId?",
    name: "StubForm",
    component: () =>
      import(/* webpackChunkName: "stubForm" */ "../views/StubForm.vue"),
  },
  {
    path: "/settings",
    name: "Settings",
    component: () =>
      import(/* webpackChunkName: "settings" */ "../views/Settings.vue"),
  },
  {
    path: "/login",
    name: "Login",
    component: () =>
      import(/* webpackChunkName: "login" */ "../views/Login.vue"),
  },
];

const router = createRouter({
  history: createWebHashHistory(),
  routes,
});

router.beforeEach((to, _, next) => {
  const authEnabled = store.getters["metadata/authenticationEnabled"];
  const authenticated = store.getters["users/getAuthenticated"];
  if (authEnabled && !authenticated && to.name !== "Login") {
    next({ name: "Login" });
  } else {
    next();
  }
});

export default router;