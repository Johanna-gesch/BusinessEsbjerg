/**
 * This file is the entry point for the React app, it sets up the root
 * element and renders the App component to the DOM.
 *
 * It is included in `src/index.html`.
 */

import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import {createBrowserRouter, Route, RouterProvider} from "react-router-dom"
import {AdminPage} from "@/FullPages/AdminPage.tsx";
import LoginPage from "@/FullPages/LoginPage.tsx";

const router = createBrowserRouter([
    {
        path: "/", element:<LoginPage/>,
    },
    {
        path: "/admin", element: <AdminPage />
    },
]);

const elem = document.getElementById("root")!;
const app = (
  <StrictMode>
      <RouterProvider router={router}/>
  </StrictMode>
);

// https://bun.com/docs/bundler/hot-reloading#import-meta-hot-data
(import.meta.hot.data.root ??= createRoot(elem)).render(app);
