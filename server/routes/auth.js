import bcrypt from "bcrypt";

import User from "../models/user";

export const loginUser = async (req, res) => {
  const user = await User.findOne({ email: req.body.email }).exec();

  if (!user) {
    return res.status(401).json({
      message: "Auth failed",
    });
  }

  const result = bcrypt;
};
