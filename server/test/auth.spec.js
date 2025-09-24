import process from "process";
import path from "path";

import mongoose from "mongoose";
import { connectDB } from "../config/database";
import server from "../server";

import { before, beforeEach, after, describe, it } from "mocha";
import chai from "chai";
import chaiHttp from "chai-http";
import { assert } from "console";

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

describe('Authentication enpoints', () => {
    it('Should create user when I set valid credentials', (done) => {
      //Arrange
      const authData = { email: "example@example.com", password: ""};

      //Act and Assert
      chai
        .request(server)
        .post(path.join(baseAuthUrl, "/signup"))
        .end((err, res) => {
            assert.equal(res.status, 201);
            assert.
            done();
        });
    }); 
});