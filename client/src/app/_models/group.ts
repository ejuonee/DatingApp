export interface Group {
  name: string;
  connections: Connection[];
}

interface Connection {
  connectionId: string;
  username: string;
}

// public class Connection(connectionId:string , username:string) {
//   this.connectionId = connectionId;
//   this.username = username;
// }
