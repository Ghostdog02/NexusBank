import bcrypt from "bcrypt";
import jwt from "jsonwebtoken";
import process from "process";

import User from "../models/user";

const internalServerErrorCode = 500;
const unauthorizedCode = 401;
const successfulCreation = 201;
const successCode = 200;

export const loginUser = async (req, res) => {
  

  try {
    const user = await User.findOne({ email: req.body.email }).exec();

    if (!user) {
      return res.status(unauthorizedCode).json({
        message: "Auth failed",
      });
    }

    const result = bcrypt.compare(req.body.password, user.password);

    if (!result) {
      return res.status(unauthorizedCode).json({
        message: "Auth failed",
      });
    }

    const token = jwt.sign(
      { email: user.email, userId: user._id },
      process.env.JWT_KEY,
      { expiresIn: "1h" }
    );

    res.status(successCode).json({
      token: token,
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
    const user = await User.findOne({ email: req.body.email }).exec();

    if (!user) {
      const hash = await bcrypt.hash(req.body.password, 10).exec();

      const user = new User({ email: req.body.email, password: hash });
      const result = await user.save().exec();

      res.status(successfulCreation).json({
        message: "Successful user creation",
        result: result
      });
    }
  } catch (error) {
    res.status(internalServerErrorCode).json({
      message: "Invalid authentication credentials!",
    });
  }
};

export default {
  loginUser,
  createUser
};
