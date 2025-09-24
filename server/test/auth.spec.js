import process from "process";
import path from "path";

import mongoose from "mongoose";
import { connectDB } from "../config/database";
import server from "../server";

import { before, beforeEach, after, describe, it } from "mocha";
import chai from "chai";
import chaiHttp from "chai-http";
import { assert } from "console";

import User from "../models/user";

const baseAuthUrl = "/api/auth";

chai.use(chaiHttp);
before(async () => {
  process.env.NODE_ENV = "test";
  await connectDB();
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

    //Act and Assert
    chai
      .request(server)
      .post(path.join(baseAuthUrl, "/signup"), authData)
      .end(async (err, res) => {
        assert.equal(res.status, 201, "Http response was different than 201");
        const user = await getUser(authData.email);
        assert.isNotNull(user, "The user was not created");
        assert.equal(
          user.email,
          authData.email,
          "The email of the newly created user was different"
        );
        assert.equal(
          user.password,
          authData.password,
          "The password of the newly created user was different"
        );
        done();
      });
  });
});

async function getUser(email) {
  return await User.findOne({ email: email }).exec();
}