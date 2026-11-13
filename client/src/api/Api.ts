/* eslint-disable */
/* tslint:disable */
// @ts-nocheck
/*
 * ---------------------------------------------------------------
 * ## THIS FILE WAS GENERATED VIA SWAGGER-TYPESCRIPT-API        ##
 * ##                                                           ##
 * ## AUTHOR: acacode                                           ##
 * ## SOURCE: https://github.com/acacode/swagger-typescript-api ##
 * ---------------------------------------------------------------
 */

export enum HolidayRequestStatus {
  Pending = 0,
  Approved = 1,
  Rejected = 2,
  Cancelled = 3,
}

export enum HolidayTransactionType {
  Earned = 0,
  Used = 1,
  TransferredIn = 2,
  TransferredOut = 3,
  PaidOut = 4,
  Adjustment = 5,
}

export enum HolidayType {
  Week15 = 0,
  Week6 = 1,
}

export interface HolidayAccount {
  /** @format guid */
  id: string;
  userId: string;
  type: HolidayType;
  earnings: HolidayEarning[];
  transactions: HolidayTransaction[];
}

export interface HolidayEarning {
  /** @format guid */
  id: string;
  /** @format guid */
  holidayAccountId: string;
  /** @format date */
  earnedDate: string;
  /** @format decimal */
  earnedHours: number;
  /** @format date */
  expiryDate: string;
}

export interface HolidayTransaction {
  /** @format guid */
  id: string;
  /** @format guid */
  holidayAccountId: string;
  /** @format guid */
  holidayEarningId?: string | null;
  /** @format decimal */
  amountHours: number;
  /** @format date */
  transactionDate: string;
  transactionType: HolidayTransactionType;
  description?: string | null;
}

export interface CreateHolidayAccountDto {
  userId: string;
  type: HolidayType;
}

export interface CreateUserDto {
  username: string;
  password: string;
  isAdmin: boolean;
}

export interface User {
  userId: string;
  username: string;
  passwordHash: string;
  isAdmin: boolean;
  isActive: boolean;
  holidayAccounts: HolidayAccount[];
  holidayRequests: HolidayRequest[];
}

export interface HolidayRequest {
  /** @format guid */
  id: string;
  userId: string;
  /** @format guid */
  holidayAccountId: string;
  /** @format date-time */
  createdAt: string;
  cancellationReason?: string | null;
  /** @format date */
  startDate: string;
  /** @format date */
  endDate: string;
  status: HolidayRequestStatus;
  approvedByUserId?: string | null;
  /** @format date-time */
  approvedAt?: string | null;
  rejectionReason?: string | null;
  user: User;
  holidayAccount: HolidayAccount;
}

export interface UpdateUserDto {
  userId: string;
  username: string;
}

export interface LoginResponseDTO {
  token: string;
}

export interface LoginRequestDTO {
  username: string;
  password: string;
}

export interface AccountGetAccountByIdParams {
  /** @format guid */
  id?: string;
}

export interface AccountGetAccountsForUserParams {
  userId?: string;
}

export interface UserGetUserByIdParams {
  id?: string;
}

export interface UserDeleteUserParams {
  userId?: string;
}

export type QueryParamsType = Record<string | number, any>;
export type ResponseFormat = keyof Omit<Body, "body" | "bodyUsed">;

export interface FullRequestParams extends Omit<RequestInit, "body"> {
  /** set parameter to `true` for call `securityWorker` for this request */
  secure?: boolean;
  /** request path */
  path: string;
  /** content type of request body */
  type?: ContentType;
  /** query params */
  query?: QueryParamsType;
  /** format of response (i.e. response.json() -> format: "json") */
  format?: ResponseFormat;
  /** request body */
  body?: unknown;
  /** base url */
  baseUrl?: string;
  /** request cancellation token */
  cancelToken?: CancelToken;
}

export type RequestParams = Omit<
  FullRequestParams,
  "body" | "method" | "query" | "path"
>;

export interface ApiConfig<SecurityDataType = unknown> {
  baseUrl?: string;
  baseApiParams?: Omit<RequestParams, "baseUrl" | "cancelToken" | "signal">;
  securityWorker?: (
    securityData: SecurityDataType | null,
  ) => Promise<RequestParams | void> | RequestParams | void;
  customFetch?: typeof fetch;
}

export interface HttpResponse<D extends unknown, E extends unknown = unknown>
  extends Response {
  data: D;
  error: E;
}

type CancelToken = Symbol | string | number;

export enum ContentType {
  Json = "application/json",
  JsonApi = "application/vnd.api+json",
  FormData = "multipart/form-data",
  UrlEncoded = "application/x-www-form-urlencoded",
  Text = "text/plain",
}

export class HttpClient<SecurityDataType = unknown> {
  public baseUrl: string = "http://localhost:5178";
  private securityData: SecurityDataType | null = null;
  private securityWorker?: ApiConfig<SecurityDataType>["securityWorker"];
  private abortControllers = new Map<CancelToken, AbortController>();
  private customFetch = (...fetchParams: Parameters<typeof fetch>) =>
    fetch(...fetchParams);

  private baseApiParams: RequestParams = {
    credentials: "same-origin",
    headers: {},
    redirect: "follow",
    referrerPolicy: "no-referrer",
  };

  constructor(apiConfig: ApiConfig<SecurityDataType> = {}) {
    Object.assign(this, apiConfig);
  }

  public setSecurityData = (data: SecurityDataType | null) => {
    this.securityData = data;
  };

  protected encodeQueryParam(key: string, value: any) {
    const encodedKey = encodeURIComponent(key);
    return `${encodedKey}=${encodeURIComponent(typeof value === "number" ? value : `${value}`)}`;
  }

  protected addQueryParam(query: QueryParamsType, key: string) {
    return this.encodeQueryParam(key, query[key]);
  }

  protected addArrayQueryParam(query: QueryParamsType, key: string) {
    const value = query[key];
    return value.map((v: any) => this.encodeQueryParam(key, v)).join("&");
  }

  protected toQueryString(rawQuery?: QueryParamsType): string {
    const query = rawQuery || {};
    const keys = Object.keys(query).filter(
      (key) => "undefined" !== typeof query[key],
    );
    return keys
      .map((key) =>
        Array.isArray(query[key])
          ? this.addArrayQueryParam(query, key)
          : this.addQueryParam(query, key),
      )
      .join("&");
  }

  protected addQueryParams(rawQuery?: QueryParamsType): string {
    const queryString = this.toQueryString(rawQuery);
    return queryString ? `?${queryString}` : "";
  }

  private contentFormatters: Record<ContentType, (input: any) => any> = {
    [ContentType.Json]: (input: any) =>
      input !== null && (typeof input === "object" || typeof input === "string")
        ? JSON.stringify(input)
        : input,
    [ContentType.JsonApi]: (input: any) =>
      input !== null && (typeof input === "object" || typeof input === "string")
        ? JSON.stringify(input)
        : input,
    [ContentType.Text]: (input: any) =>
      input !== null && typeof input !== "string"
        ? JSON.stringify(input)
        : input,
    [ContentType.FormData]: (input: any) => {
      if (input instanceof FormData) {
        return input;
      }

      return Object.keys(input || {}).reduce((formData, key) => {
        const property = input[key];
        formData.append(
          key,
          property instanceof Blob
            ? property
            : typeof property === "object" && property !== null
              ? JSON.stringify(property)
              : `${property}`,
        );
        return formData;
      }, new FormData());
    },
    [ContentType.UrlEncoded]: (input: any) => this.toQueryString(input),
  };

  protected mergeRequestParams(
    params1: RequestParams,
    params2?: RequestParams,
  ): RequestParams {
    return {
      ...this.baseApiParams,
      ...params1,
      ...(params2 || {}),
      headers: {
        ...(this.baseApiParams.headers || {}),
        ...(params1.headers || {}),
        ...((params2 && params2.headers) || {}),
      },
    };
  }

  protected createAbortSignal = (
    cancelToken: CancelToken,
  ): AbortSignal | undefined => {
    if (this.abortControllers.has(cancelToken)) {
      const abortController = this.abortControllers.get(cancelToken);
      if (abortController) {
        return abortController.signal;
      }
      return void 0;
    }

    const abortController = new AbortController();
    this.abortControllers.set(cancelToken, abortController);
    return abortController.signal;
  };

  public abortRequest = (cancelToken: CancelToken) => {
    const abortController = this.abortControllers.get(cancelToken);

    if (abortController) {
      abortController.abort();
      this.abortControllers.delete(cancelToken);
    }
  };

  public request = async <T = any, E = any>({
    body,
    secure,
    path,
    type,
    query,
    format,
    baseUrl,
    cancelToken,
    ...params
  }: FullRequestParams): Promise<T> => {
    const secureParams =
      ((typeof secure === "boolean" ? secure : this.baseApiParams.secure) &&
        this.securityWorker &&
        (await this.securityWorker(this.securityData))) ||
      {};
    const requestParams = this.mergeRequestParams(params, secureParams);
    const queryString = query && this.toQueryString(query);
    const payloadFormatter = this.contentFormatters[type || ContentType.Json];
    const responseFormat = format || requestParams.format;

    return this.customFetch(
      `${baseUrl || this.baseUrl || ""}${path}${queryString ? `?${queryString}` : ""}`,
      {
        ...requestParams,
        headers: {
          ...(requestParams.headers || {}),
          ...(type && type !== ContentType.FormData
            ? { "Content-Type": type }
            : {}),
        },
        signal:
          (cancelToken
            ? this.createAbortSignal(cancelToken)
            : requestParams.signal) || null,
        body:
          typeof body === "undefined" || body === null
            ? null
            : payloadFormatter(body),
      },
    ).then(async (response) => {
      const r = response as HttpResponse<T, E>;
      r.data = null as unknown as T;
      r.error = null as unknown as E;

      const responseToParse = responseFormat ? response.clone() : response;
      const data = !responseFormat
        ? r
        : await responseToParse[responseFormat]()
            .then((data) => {
              if (r.ok) {
                r.data = data;
              } else {
                r.error = data;
              }
              return r;
            })
            .catch((e) => {
              r.error = e;
              return r;
            });

      if (cancelToken) {
        this.abortControllers.delete(cancelToken);
      }

      if (!response.ok) throw data;
      return data.data;
    });
  };
}

/**
 * @title My Title
 * @version 1.0.0
 * @baseUrl http://localhost:5178
 */
export class Api<
  SecurityDataType extends unknown,
> extends HttpClient<SecurityDataType> {
  createAccount = {
    /**
     * No description
     *
     * @tags Account
     * @name AccountCreateAccount
     * @request POST:/CreateAccount
     * @secure
     */
    accountCreateAccount: (
      data: CreateHolidayAccountDto,
      params: RequestParams = {},
    ) =>
      this.request<HolidayAccount, any>({
        path: `/CreateAccount`,
        method: "POST",
        body: data,
        secure: true,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),
  };
  getAccountById = {
    /**
     * No description
     *
     * @tags Account
     * @name AccountGetAccountById
     * @request GET:/GetAccountById
     * @secure
     */
    accountGetAccountById: (
      query: AccountGetAccountByIdParams = {},
      params: RequestParams = {},
    ) =>
      this.request<HolidayAccount, any>({
        path: `/GetAccountById`,
        method: "GET",
        query: query,
        secure: true,
        format: "json",
        ...params,
      }),
  };
  getAccountsForUser = {
    /**
     * No description
     *
     * @tags Account
     * @name AccountGetAccountsForUser
     * @request GET:/GetAccountsForUser
     * @secure
     */
    accountGetAccountsForUser: (
      query: AccountGetAccountsForUserParams = {},
      params: RequestParams = {},
    ) =>
      this.request<HolidayAccount[], any>({
        path: `/GetAccountsForUser`,
        method: "GET",
        query: query,
        secure: true,
        format: "json",
        ...params,
      }),
  };
  createUser = {
    /**
     * No description
     *
     * @tags User
     * @name UserCreateUser
     * @request POST:/CreateUser
     * @secure
     */
    userCreateUser: (data: CreateUserDto, params: RequestParams = {}) =>
      this.request<void, any>({
        path: `/CreateUser`,
        method: "POST",
        body: data,
        secure: true,
        type: ContentType.Json,
        ...params,
      }),
  };
  getUsers = {
    /**
     * No description
     *
     * @tags User
     * @name UserGetUsers
     * @request GET:/GetUsers
     * @secure
     */
    userGetUsers: (params: RequestParams = {}) =>
      this.request<User[], any>({
        path: `/GetUsers`,
        method: "GET",
        secure: true,
        format: "json",
        ...params,
      }),
  };
  getUserById = {
    /**
     * No description
     *
     * @tags User
     * @name UserGetUserById
     * @request GET:/GetUserById
     * @secure
     */
    userGetUserById: (
      query: UserGetUserByIdParams = {},
      params: RequestParams = {},
    ) =>
      this.request<User, any>({
        path: `/GetUserById`,
        method: "GET",
        query: query,
        secure: true,
        format: "json",
        ...params,
      }),
  };
  updateUser = {
    /**
     * No description
     *
     * @tags User
     * @name UserUpdateUser
     * @request PUT:/UpdateUser
     * @secure
     */
    userUpdateUser: (data: UpdateUserDto, params: RequestParams = {}) =>
      this.request<void, any>({
        path: `/UpdateUser`,
        method: "PUT",
        body: data,
        secure: true,
        type: ContentType.Json,
        ...params,
      }),
  };
  deleteUser = {
    /**
     * No description
     *
     * @tags User
     * @name UserDeleteUser
     * @request DELETE:/DeleteUser
     * @secure
     */
    userDeleteUser: (
      query: UserDeleteUserParams = {},
      params: RequestParams = {},
    ) =>
      this.request<void, any>({
        path: `/DeleteUser`,
        method: "DELETE",
        query: query,
        secure: true,
        ...params,
      }),
  };
  login = {
    /**
     * No description
     *
     * @tags User
     * @name UserLogin
     * @request POST:/login
     * @secure
     */
    userLogin: (data: LoginRequestDTO, params: RequestParams = {}) =>
      this.request<LoginResponseDTO, any>({
        path: `/login`,
        method: "POST",
        body: data,
        secure: true,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),
  };
}
