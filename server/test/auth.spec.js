import process from "process";
import path from "path";

import mongoose from "mongoose";
import { connectDB } from "../config/database.js";
import server from "../server.js";

import { before, beforeEach, after, describe, it } from "mocha";
import { use } from "chai";
import { default as chaiHttp, request } from "chai-http";
import { assert } from "console";

import User from "../models/user.js";

const baseAuthUrl = "/api/auth";

use(chaiHttp);

before(async () => {
  process.env.NODE_ENV = "test";

  await connectDB()
    .then(() => {
      console.log("Connected to database");
    })
    .catch(() => {
      console.log("Connection failed!");
    });
});

beforeEach(async () => {
  // Clean database between tests
  const collections = mongoose.connection.collections;
  for (const key in collections) {
    await collections[key].deleteMany({});
  }
});

after(async () => {
  await mongoose.connection.dropDatabase();
  await mongoose.connection.close();
  process.env.NODE_ENV = "development";
});

describe("Successful User Creation", () => {
  it("should create a new user when email does not exist", (done) => {
    //Arrange
    const authData = {
      email: "example@example.com",
      password: "uiAzbnW!zIj1!a",
    };

    request.execute(server)
      //Act
      .post(path.join(baseAuthUrl, "/signup"))
      .send(authData)
      .end(async (err, res) => {
        //Assert
        assert.equal(res.status, 201, "Http response was different than 201");
        const user = await getUser(authData.email);
        assert.isNotNull(user, "The user was not created");
        assert.equal(
          user.email,
          authData.email,
          "The email of the newly created user was different"
        );
        assert.notEqual(
          user.password,
          authData.password,
          "Password should be hashed"
        );
        done();
      });
  });
});

async function getUser(email) {
  return await User.findOne({ email: email }).exec();
}