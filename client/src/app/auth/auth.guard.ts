import { inject } from "@angular/core";
import { ActivatedRouteSnapshot, GuardResult, MaybeAsync, Router, RouterStateSnapshot } from "@angular/router";
import { AuthService } from "./auth.service";

export function authGuard(route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot
): MaybeAsync<GuardResult> {
    const router = inject(Router);
    const isAuth = inject(AuthService).getIsAuth();

    if (!isAuth) {
        router.navigate(["/auth/login"]);
    }

    return isAuth;
}