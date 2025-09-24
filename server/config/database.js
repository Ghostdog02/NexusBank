import mongoose from "mongoose";
import process from "process";

export const connectDB = async () => {
  const dbName = process.env.NODE_ENV === "test" ? "BankTest" : "Bank";

  const configuration = {
    dbName: dbName,
    directConnection: true,
    serverSelectionTimeoutMS: 2000,
    appName: "mongosh 2.5.7",
  };

  await mongoose.connect(process.env.MONGO_URI, configuration);
};
