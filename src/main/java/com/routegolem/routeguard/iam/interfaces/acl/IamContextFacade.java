package com.routegolem.routeguard.iam.interfaces.acl;
import com.routegolem.routeguard.iam.domain.model.aggregates.User;
import com.routegolem.routeguard.iam.domain.model.commands.SignUpCommand;
import com.routegolem.routeguard.iam.domain.model.queries.GetUserByIdQuery;
import com.routegolem.routeguard.iam.domain.model.queries.GetUserByUsernameQuery;
import com.routegolem.routeguard.iam.domain.services.UserCommandService;
import com.routegolem.routeguard.iam.domain.services.UserQueryService;
import org.springframework.stereotype.Service;
import java.util.List;
@Service
public class IamContextFacade {
    private final UserCommandService userCommandService;
    private final UserQueryService userQueryService;
    public IamContextFacade(UserCommandService userCommandService, UserQueryService userQueryService) {
        this.userCommandService = userCommandService;
        this.userQueryService = userQueryService;
    }
    public Long createUser(String username, String password, List<String> roles) {
        var signUpCommand = new SignUpCommand(username, password, roles);
        var result = userCommandService.handle(signUpCommand);
        if (result.isEmpty()) return 0L;
        return result.get().getId();
    }
    public Long fetchUserIdByUsername(String username) {
        var getUserQuery = new GetUserByUsernameQuery(username);
        var result = userQueryService.handle(getUserQuery);
        if (result.isEmpty()) return 0L;
        return result.get().getId();
    }
    public String fetchUsernameByUserId(Long userId) {
        var getUserQuery = new GetUserByIdQuery(userId);
        var result = userQueryService.handle(getUserQuery);
        if (result.isEmpty()) return "";
        return result.get().getUsername();
    }
}
