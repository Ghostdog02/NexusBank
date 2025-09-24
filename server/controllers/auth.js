import bcrypt from "bcrypt";
import jwt from "jsonwebtoken";
import process from "process";

import User from "../models/user.js";

const successfulCreation = 201;
const successCode = 200;
const unauthorizedCode = 401;
const internalServerErrorCode = 500;

export const logInUser = async (req, res) => {
  try {
    const user = await User.findOne({ email: req.body.email }).exec();

    if (!user) {
      return res.status(unauthorizedCode).json({
        message: "User not found in database",
      });
    }

    console.log(user);

    const result = bcrypt.compare(req.body.password, user.password);

    if (!result) {
      return res.status(unauthorizedCode).json({
        message: "Auth failed",
      });
    }

    const jwtToken = jwt.sign(
      { email: user.email, userId: user._id },
      process.env.JWT_KEY,
      { expiresIn: "1h" }
    );

    const cookieOptions = {
      httpOnly: true,
      sameSite: "strict", // CSRF protection
      // maxAge: 60 * 60 * 1000, // 1 hour in ms
    };

    res
      .status(successCode)
      .cookie("auth_token", jwtToken, cookieOptions)
      .json({
        message: "Authenticated",
        expiresIn: 3600,
        userId: user._id,
      });

  } catch (error) {
    return res.status(unauthorizedCode).json({
      message: "Invalid authentication credentials",
    });
  }
};

export const createUser = async (req, res) => {
  try {
    let user = await User.findOne({ email: req.body.email }).exec();

    if (!user) {
      const hash = await bcrypt.hash(req.body.password, 10);

      user = new User({
        email: req.body.email,
        password: hash,
        role: "Customer",
      });
    }

    const result = await user.save();

    res.status(successfulCreation).json({
      message: "Successful user creation",
      result: result,
    });

    console.log(user);

    // eslint-disable-next-line no-unused-vars
  } catch (error) {
    res.status(internalServerErrorCode).json({
      message: "Invalid authentication credentials!",
    });
  }
};

export const logInUserWithGoogle = async (req, res) => {
  try {
    console.log();
  } catch (error) {
    res.status(internalServerErrorCode).json({
      message: "Method call failed",
    });
  }
};

export default {
  logInUser,
  createUser,
  logInUserWithGoogle,
};
