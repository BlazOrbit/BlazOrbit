// SnapshotTests/TypeScriptSnapshotTests.cs
using BlazOrbit.SyntaxHighlight.Languages;
using BlazOrbit.SyntaxHighlight.Tokens;

namespace BlazOrbit.SyntaxHighlight.Tests.SnapshotTests;

public class TypeScriptSnapshotTests
{
    [Fact]
    public Task Tokenize_CompleteModule_MatchesSnapshot()
    {
        string code = """
            import { Injectable, OnDestroy } from '@angular/core';
            import { HttpClient, HttpErrorResponse } from '@angular/common/http';
            import { BehaviorSubject, Observable, Subject, throwError } from 'rxjs';
            import { catchError, map, retry, takeUntil, tap } from 'rxjs/operators';

            // Types and Interfaces
            export interface User {
                id: number;
                email: string;
                firstName: string;
                lastName: string;
                role: UserRole;
                createdAt: Date;
                metadata?: Record<string, unknown>;
            }

            export type UserRole = 'admin' | 'user' | 'guest';

            export interface ApiResponse<T> {
                data: T;
                message: string;
                timestamp: number;
            }

            export interface PaginatedResponse<T> extends ApiResponse<T[]> {
                total: number;
                page: number;
                pageSize: number;
                hasMore: boolean;
            }

            type CreateUserDto = Omit<User, 'id' | 'createdAt'>;
            type UpdateUserDto = Partial<CreateUserDto>;

            // Constants
            const API_BASE_URL = 'https://api.example.com/v1';
            const MAX_RETRIES = 3;
            const CACHE_DURATION_MS = 5 * 60 * 1000; // 5 minutes

            /**
             * Service for managing user data with caching and error handling.
             * @example
             * ```typescript
             * const userService = new UserService(httpClient);
             * const users = await userService.getUsers().toPromise();
             * ```
             */
            @Injectable({
                providedIn: 'root'
            })
            export class UserService implements OnDestroy {
                private readonly destroy$ = new Subject<void>();
                private readonly usersCache$ = new BehaviorSubject<User[] | null>(null);
                private lastFetchTime: number = 0;

                constructor(private readonly http: HttpClient) {}

                ngOnDestroy(): void {
                    this.destroy$.next();
                    this.destroy$.complete();
                }

                /**
                 * Retrieves all users with optional caching.
                 * @param forceRefresh - If true, bypasses cache
                 * @returns Observable of user array
                 */
                getUsers(forceRefresh = false): Observable<User[]> {
                    const now = Date.now();
                    const cacheValid = now - this.lastFetchTime < CACHE_DURATION_MS;

                    if (!forceRefresh && cacheValid && this.usersCache$.value) {
                        return this.usersCache$.asObservable() as Observable<User[]>;
                    }

                    return this.http.get<ApiResponse<User[]>>(`${API_BASE_URL}/users`).pipe(
                        retry(MAX_RETRIES),
                        map(response => response.data),
                        tap(users => {
                            this.usersCache$.next(users);
                            this.lastFetchTime = Date.now();
                        }),
                        catchError(this.handleError),
                        takeUntil(this.destroy$)
                    );
                }

                /**
                 * Retrieves a single user by ID.
                 */
                getUserById(id: number): Observable<User> {
                    if (id <= 0) {
                        return throwError(() => new Error('Invalid user ID'));
                    }

                    // Check cache first
                    const cachedUser = this.usersCache$.value?.find(u => u.id === id);
                    if (cachedUser) {
                        return new Observable(subscriber => {
                            subscriber.next(cachedUser);
                            subscriber.complete();
                        });
                    }

                    return this.http.get<ApiResponse<User>>(`${API_BASE_URL}/users/${id}`).pipe(
                        map(response => response.data),
                        catchError(this.handleError)
                    );
                }

                /**
                 * Creates a new user.
                 */
                async createUser(userData: CreateUserDto): Promise<User> {
                    const response = await this.http
                        .post<ApiResponse<User>>(`${API_BASE_URL}/users`, userData)
                        .toPromise();

                    if (!response) {
                        throw new Error('Failed to create user');
                    }

                    // Update cache
                    const currentUsers = this.usersCache$.value ?? [];
                    this.usersCache$.next([...currentUsers, response.data]);

                    return response.data;
                }

                /**
                 * Updates an existing user.
                 */
                updateUser(id: number, updates: UpdateUserDto): Observable<User> {
                    return this.http.patch<ApiResponse<User>>(`${API_BASE_URL}/users/${id}`, updates).pipe(
                        map(response => response.data),
                        tap(updatedUser => {
                            const users = this.usersCache$.value;
                            if (users) {
                                const index = users.findIndex(u => u.id === id);
                                if (index !== -1) {
                                    users[index] = updatedUser;
                                    this.usersCache$.next([...users]);
                                }
                            }
                        }),
                        catchError(this.handleError)
                    );
                }

                /**
                 * Deletes a user by ID.
                 */
                deleteUser(id: number): Observable<void> {
                    return this.http.delete<void>(`${API_BASE_URL}/users/${id}`).pipe(
                        tap(() => {
                            const users = this.usersCache$.value;
                            if (users) {
                                this.usersCache$.next(users.filter(u => u.id !== id));
                            }
                        }),
                        catchError(this.handleError)
                    );
                }

                /**
                 * Searches users by query string.
                 */
                searchUsers(query: string, options?: {
                    page?: number;
                    pageSize?: number;
                    sortBy?: keyof User;
                    sortOrder?: 'asc' | 'desc';
                }): Observable<PaginatedResponse<User>> {
                    const params = new URLSearchParams({
                        q: query,
                        page: String(options?.page ?? 1),
                        pageSize: String(options?.pageSize ?? 20),
                        ...(options?.sortBy && { sortBy: options.sortBy }),
                        ...(options?.sortOrder && { sortOrder: options.sortOrder })
                    });

                    return this.http
                        .get<PaginatedResponse<User>>(`${API_BASE_URL}/users/search?${params}`)
                        .pipe(catchError(this.handleError));
                }

                /**
                 * Handles HTTP errors.
                 */
                private handleError = (error: HttpErrorResponse): Observable<never> => {
                    let errorMessage: string;

                    if (error.error instanceof ErrorEvent) {
                        // Client-side error
                        errorMessage = `Client error: ${error.error.message}`;
                    } else {
                        // Server-side error
                        errorMessage = `Server error: ${error.status} - ${error.statusText}`;

                        switch (error.status) {
                            case 401:
                                errorMessage = 'Unauthorized. Please log in again.';
                                break;

                            case 403:
                                errorMessage = 'Access forbidden.';
                                break;

                            case 404:
                                errorMessage = 'Resource not found.';
                                break;

                            case 500:
                                errorMessage = 'Internal server error. Please try again later.';
                                break;
                        }
                    }

                    console.error('UserService Error:', errorMessage, error);
                    return throwError(() => new Error(errorMessage));
                };

                /**
                 * Clears the user cache.
                 */
                clearCache(): void {
                    this.usersCache$.next(null);
                    this.lastFetchTime = 0;
                }
            }

            // Utility functions
            export const formatUserName = (user: User): string =>
                `${user.firstName} ${user.lastName}`.trim();

            export const isAdmin = (user: User): boolean => user.role === 'admin';

            export const filterActiveUsers = <T extends { active?: boolean }>(users: T[]): T[] =>
                users.filter(u => u.active !== false);
            """;

        IReadOnlyList<Token> tokens = TypeScriptLanguage.Instance.Tokenize(code);

        return Verify(tokens.Select(t => new { t.Type, t.Value }));
    }
}