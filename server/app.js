import createError from 'http-errors';
import express, { json, urlencoded } from 'express';
import cookieParser from 'cookie-parser';
import logger from 'morgan';
import mongoose from 'mongoose';
import cors from 'cors';

import authRoutes from './routes/auth.js';

var app = express();

mongoose
  .connect(
    `mongodb://127.0.0.1:27017`, {
      dbName: "Bank",
      directConnection: true,
      serverSelectionTimeoutMS: 2000,
      appName: "mongosh 2.5.7"
    }
  )
  .then(() => {
    console.log("Connected to database");
  })
  .catch(() => {
    console.log("Connection failed!");
  });

app.use(logger('dev'));
app.use(json());
app.use(urlencoded({ extended: false }));
app.use(cookieParser());

const corsOptions = {
  origin: ["http://localhost:3000", "http://localhost:4200"],
  credentials: true
};

app.use(cors(corsOptions));

app.use("/api/auth", authRoutes);

app.use(function(req, res, next) {
  next(createError(404));
});

// error handler
app.use(function(err, req, res) {
  // set locals, only providing error in development
  res.locals.message = err.message;
  res.locals.error = req.app.get('env') === 'development' ? err : {};

  res.status(err.status || 500);
  res.render('error');
});

export default app;
