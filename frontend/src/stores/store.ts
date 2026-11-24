import { configureStore } from "@reduxjs/toolkit";

// ponytail: giữ một reducer gốc cho đến khi màn hình đầu tiên cần state dùng chung.
export const store = configureStore({ reducer: { app: (state = {}) => state } });
export type RootState = ReturnType<typeof store.getState>;
export type AppDispatch = typeof store.dispatch;
