import mongoose from "mongoose";
import process from "process";

export const connectDB = async () => {
  const mongoURI =
    process.env.NODE_ENV === "test"
      ? process.env.MONGO_TEST_URI || "mongodb://localhost:27017/myapp_test"
      : process.env.MONGO_URI || "mongodb://localhost:27017/myapp";
  const dbName = process.env.NODE_ENV === "test" ? "BankTest" : "Bank";

  const configuration = {
    dbName: dbName,
    directConnection: true,
    serverSelectionTimeoutMS: 2000,
    appName: "mongosh 2.5.7",
  };

  await mongoose.connect(mongoURI, configuration);
};
