import express from "express";

import AuthController from "../controllers/auth.js";

const router = express.Router();

router.post("/signup", AuthController.createUser);

router.post("/login", AuthController.logInUser);

router.post("google-login", AuthController.logInGoogle);

export default router;
