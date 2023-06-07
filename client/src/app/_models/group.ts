export interface Group {
  name: string;
  conncetions: Connection[];
}

export interface Connection {
  connectionId: string;
  username: string;
}
